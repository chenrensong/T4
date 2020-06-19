using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Platform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal class TelemetryConfigurationFactory
	{
		private const string AddElementName = "Add";

		private const string TypeAttributeName = "Type";

		private static readonly MethodInfo LoadInstancesDefinition = typeof(TelemetryConfigurationFactory).GetRuntimeMethods().First((MethodInfo m) => m.Name == "LoadInstances");

		private static readonly XNamespace XmlNamespace = "http://schemas.microsoft.com/ApplicationInsights/2013/Settings";

		private static TelemetryConfigurationFactory instance;

		/// <summary>
		/// Gets or sets the default <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.TelemetryConfigurationFactory" /> instance used by <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration" />.
		/// </summary>
		/// <remarks>
		/// This property is a test isolation "pinch point" that allows us to test <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.TelemetryConfiguration" /> without using reflection.
		/// </remarks>
		public static TelemetryConfigurationFactory Instance
		{
			get
			{
				return instance ?? (instance = new TelemetryConfigurationFactory());
			}
			set
			{
				instance = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.TelemetryConfigurationFactory" /> class.
		/// </summary>
		/// <remarks>
		/// This constructor is protected because <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.TelemetryConfigurationFactory" /> is only meant to be instantiated
		/// by the <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.TelemetryConfigurationFactory.Instance" /> property or by tests.
		/// </remarks>
		protected TelemetryConfigurationFactory()
		{
		}

		public virtual void Initialize(TelemetryConfiguration configuration)
		{
			configuration.ContextInitializers.Add(new SdkVersionPropertyContextInitializer());
			configuration.TelemetryInitializers.Add(new TimestampPropertyInitializer());
			string text = PlatformSingleton.Current.ReadConfigurationXml();
			if (!string.IsNullOrEmpty(text))
			{
				XDocument xml = XDocument.Parse(text);
				LoadFromXml(configuration, xml);
			}
			InitializeComponents(configuration);
		}

		protected static object CreateInstance(Type interfaceType, string typeName)
		{
			Type type = GetType(typeName);
			if (type == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Type '{0}' could not be loaded.", new object[1]
				{
					typeName
				}));
			}
			object obj = Activator.CreateInstance(type);
			if (!interfaceType.IsAssignableFrom(obj.GetType()))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Type '{0}' does not implement the required interface {1}.", new object[2]
				{
					type.AssemblyQualifiedName,
					interfaceType.FullName
				}));
			}
			return obj;
		}

		protected static void LoadFromXml(TelemetryConfiguration configuration, XDocument xml)
		{
			LoadInstance(xml.Element(XmlNamespace + "ApplicationInsights"), typeof(TelemetryConfiguration), configuration);
		}

		protected static object LoadInstance(XElement definition, Type expectedType, object instance)
		{
			if (definition != null)
			{
				XAttribute xAttribute = definition.Attribute("Type");
				if (xAttribute != null)
				{
					if (instance == null || instance.GetType() != GetType(xAttribute.Value))
					{
						instance = CreateInstance(expectedType, xAttribute.Value);
					}
				}
				else if (!definition.Elements().Any() && !definition.Attributes().Any())
				{
					LoadInstanceFromValue(definition, expectedType, ref instance);
				}
				else if (instance == null && !expectedType.IsAbstract())
				{
					instance = Activator.CreateInstance(expectedType);
				}
				else if (instance == null)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "'{0}' element does not have a Type attribute, does not specify a value and is not a valid collection type", new object[1]
					{
						definition.Name.LocalName
					}));
				}
				if (instance != null)
				{
					LoadProperties(definition, instance);
					if (GetCollectionElementType(instance.GetType(), out Type elementType))
					{
						LoadInstancesDefinition.MakeGenericMethod(elementType).Invoke(null, new object[2]
						{
							definition,
							instance
						});
					}
				}
			}
			return instance;
		}

		protected static void LoadInstances<T>(XElement definition, ICollection<T> instances)
		{
			if (definition != null)
			{
				foreach (XElement item in definition.Elements(XmlNamespace + "Add"))
				{
					object obj = null;
					XAttribute xAttribute = item.Attribute("Type");
					if (xAttribute != null)
					{
						Type type = GetType(xAttribute.Value);
						obj = instances.FirstOrDefault((T i) => i.GetType() == type);
					}
					bool num = obj == null;
					obj = LoadInstance(item, typeof(T), obj);
					if (num)
					{
						instances.Add((T)obj);
					}
				}
			}
		}

		protected static void LoadProperties(XElement instanceDefinition, object instance)
		{
			List<XElement> list = GetPropertyDefinitions(instanceDefinition).ToList();
			if (list.Count > 0)
			{
				Type type = instance.GetType();
				Dictionary<string, PropertyInfo> dictionary = type.GetProperties().ToDictionary((PropertyInfo p) => p.Name);
				foreach (XElement item in list)
				{
					string localName = item.Name.LocalName;
					if (dictionary.TryGetValue(localName, out PropertyInfo value))
					{
						object value2 = value.GetValue(instance, null);
						value2 = LoadInstance(item, value.PropertyType, value2);
						if (value.CanWrite)
						{
							value.SetValue(instance, value2, null);
						}
					}
					else if (!(instance is TelemetryConfiguration))
					{
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "'{0}' is not a valid property name for type {1}.", new object[2]
						{
							localName,
							type.AssemblyQualifiedName
						}));
					}
				}
			}
		}

		private static void InitializeComponents(TelemetryConfiguration configuration)
		{
			InitializeComponent(configuration.TelemetryChannel, configuration);
			InitializeComponents(configuration.TelemetryModules, configuration);
			InitializeComponents(configuration.TelemetryInitializers, configuration);
			InitializeComponents(configuration.ContextInitializers, configuration);
		}

		private static void InitializeComponents(IEnumerable components, TelemetryConfiguration configuration)
		{
			foreach (object component in components)
			{
				InitializeComponent(component, configuration);
			}
		}

		private static void InitializeComponent(object component, TelemetryConfiguration configuration)
		{
			(component as ISupportConfiguration)?.Initialize(configuration);
		}

		private static void LoadInstanceFromValue(XElement definition, Type expectedType, ref object instance)
		{
			if (string.IsNullOrEmpty(definition.Value))
			{
				instance = (typeof(ValueType).IsAssignableFrom(expectedType) ? Activator.CreateInstance(expectedType) : null);
			}
			else
			{
				try
				{
					instance = Convert.ChangeType(definition.Value.Trim(), expectedType, CultureInfo.InvariantCulture);
				}
				catch (InvalidCastException innerException)
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "'{0}' element has unexpected contents: '{1}'.", new object[2]
					{
						definition.Name.LocalName,
						definition.Value
					}), innerException);
				}
			}
		}

		private static Type GetType(string typeName)
		{
			return GetManagedType(typeName);
		}

		private static Type GetWindowsRuntimeType(string typeName)
		{
			try
			{
				return Type.GetType(typeName + ", ContentType=WindowsRuntime");
			}
			catch (IOException)
			{
				return null;
			}
		}

		private static Type GetManagedType(string typeName)
		{
			try
			{
				return Type.GetType(typeName);
			}
			catch (IOException)
			{
				return null;
			}
		}

		private static bool GetCollectionElementType(Type type, out Type elementType)
		{
			Type type2 = type.GetInterfaces().FirstOrDefault((Type i) => i.IsGenericType() && i.GetGenericTypeDefinition() == typeof(ICollection<>));
			elementType = ((type2 != null) ? type2.GetGenericArguments()[0] : null);
			return elementType != null;
		}

		private static IEnumerable<XElement> GetPropertyDefinitions(XElement instanceDefinition)
		{
			IEnumerable<XElement> first = from a in instanceDefinition.Attributes()
				where !a.IsNamespaceDeclaration && a.Name.LocalName != "Type"
				select new XElement(a.Name, a.Value);
			IEnumerable<XElement> second = from e in instanceDefinition.Elements()
				where e.Name.LocalName != "Add"
				select e;
			return first.Concat(second);
		}
	}
}

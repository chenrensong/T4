using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// The reader is platform specific and applies to .NET applications only.
	/// </summary>
	/// <summary>
	/// The reader is platform specific and will contain different implementations for reading specific data based on the platform its running on.
	/// </summary>
	internal class ComponentContextReader : IComponentContextReader
	{
		/// <summary>
		/// The default application version we will be returning if no application version is found.
		/// </summary>
		internal const string UnknownComponentVersion = "Unknown";

		/// <summary>
		/// The singleton instance for our reader.
		/// </summary>
		private static IComponentContextReader instance;

		/// <summary>
		/// Gets or sets the singleton instance for our application context reader.
		/// </summary>
		public static IComponentContextReader Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}
				Interlocked.CompareExchange(ref instance, new ComponentContextReader(), null);
				instance.Initialize();
				return instance;
			}
			internal set
			{
				instance = value;
			}
		}

		/// <summary>
		/// Initializes the current reader with respect to its environment.
		/// </summary>
		public void Initialize()
		{
		}

		/// <summary>
		/// Gets the version for the current application. If the version cannot be found, we will return the passed in default.
		/// </summary>
		/// <returns>The extracted data.</returns>
		public string GetVersion()
		{
			return string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.ComponentContextReader" /> class.
		/// </summary>
		internal ComponentContextReader()
		{
		}
	}
}

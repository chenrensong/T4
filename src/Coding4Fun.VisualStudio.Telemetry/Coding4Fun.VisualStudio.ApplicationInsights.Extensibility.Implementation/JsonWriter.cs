using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal class JsonWriter : IJsonWriter
	{
		private sealed class EmptyObjectDetector : IJsonWriter
		{
			public bool IsEmpty
			{
				get;
				set;
			}

			public void WriteStartArray()
			{
			}

			public void WriteStartObject()
			{
			}

			public void WriteEndArray()
			{
			}

			public void WriteEndObject()
			{
			}

			public void WriteComma()
			{
			}

			public void WriteProperty(string name, string value)
			{
				if (!string.IsNullOrEmpty(value))
				{
					IsEmpty = false;
				}
			}

			public void WriteProperty(string name, bool? value)
			{
				if (value.HasValue)
				{
					IsEmpty = false;
				}
			}

			public void WriteProperty(string name, int? value)
			{
				if (value.HasValue)
				{
					IsEmpty = false;
				}
			}

			public void WriteProperty(string name, double? value)
			{
				if (value.HasValue)
				{
					IsEmpty = false;
				}
			}

			public void WriteProperty(string name, TimeSpan? value)
			{
				if (value.HasValue)
				{
					IsEmpty = false;
				}
			}

			public void WriteProperty(string name, DateTimeOffset? value)
			{
				if (value.HasValue)
				{
					IsEmpty = false;
				}
			}

			public void WriteProperty(string name, IJsonSerializable value)
			{
				value?.Serialize(this);
			}

			public void WriteProperty(string name, IDictionary<string, double> value)
			{
				if (value != null && value.Count > 0)
				{
					IsEmpty = false;
				}
			}

			public void WriteProperty(string name, IDictionary<string, string> value)
			{
				if (value != null && value.Count > 0)
				{
					IsEmpty = false;
				}
			}

			public void WritePropertyName(string name)
			{
			}

			public void WriteRawValue(object value)
			{
				if (value != null)
				{
					IsEmpty = false;
				}
			}
		}

		private readonly EmptyObjectDetector emptyObjectDetector;

		private readonly TextWriter textWriter;

		private bool currentObjectHasProperties;

		internal JsonWriter(TextWriter textWriter)
		{
			emptyObjectDetector = new EmptyObjectDetector();
			this.textWriter = textWriter;
		}

		public void WriteStartArray()
		{
			textWriter.Write('[');
		}

		public void WriteStartObject()
		{
			textWriter.Write('{');
			currentObjectHasProperties = false;
		}

		public void WriteEndArray()
		{
			textWriter.Write(']');
		}

		public void WriteEndObject()
		{
			textWriter.Write('}');
		}

		public void WriteComma()
		{
			textWriter.Write(',');
		}

		public void WriteRawValue(object value)
		{
			textWriter.Write(string.Format(CultureInfo.InvariantCulture, "{0}", new object[1]
			{
				value
			}));
		}

		public void WriteProperty(string name, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				WritePropertyName(name);
				WriteString(value);
			}
		}

		public void WriteProperty(string name, bool? value)
		{
			if (value.HasValue)
			{
				WritePropertyName(name);
				textWriter.Write(value.Value ? "true" : "false");
			}
		}

		public void WriteProperty(string name, int? value)
		{
			if (value.HasValue)
			{
				WritePropertyName(name);
				textWriter.Write(value.Value.ToString(CultureInfo.InvariantCulture));
			}
		}

		public void WriteProperty(string name, double? value)
		{
			if (value.HasValue)
			{
				WritePropertyName(name);
				textWriter.Write(value.Value.ToString(CultureInfo.InvariantCulture));
			}
		}

		public void WriteProperty(string name, TimeSpan? value)
		{
			if (value.HasValue)
			{
				WriteProperty(name, value.Value.ToString(CultureInfo.InvariantCulture, string.Empty));
			}
		}

		public void WriteProperty(string name, DateTimeOffset? value)
		{
			if (value.HasValue)
			{
				WriteProperty(name, value.Value.ToString("o", CultureInfo.InvariantCulture));
			}
		}

		public void WriteProperty(string name, IJsonSerializable value)
		{
			if (!IsNullOrEmpty(value))
			{
				WritePropertyName(name);
				value.Serialize(this);
			}
		}

		public void WriteProperty(string name, IDictionary<string, double> values)
		{
			if (values != null && values.Count > 0)
			{
				WritePropertyName(name);
				WriteStartObject();
				foreach (KeyValuePair<string, double> value in values)
				{
					WriteProperty(value.Key, value.Value);
				}
				WriteEndObject();
			}
		}

		public void WriteProperty(string name, IDictionary<string, string> values)
		{
			if (values != null && values.Count > 0)
			{
				WritePropertyName(name);
				WriteStartObject();
				foreach (KeyValuePair<string, string> value in values)
				{
					if (value.Value != null)
					{
						WriteProperty(value.Key, value.Value);
					}
				}
				WriteEndObject();
			}
		}

		/// <summary>
		/// Writes the specified property name enclosed in double quotation marks followed by a colon.
		/// </summary>
		/// <remarks>
		/// When this method is called multiple times, the second call after <see cref="M:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.JsonWriter.WriteStartObject" />
		/// and all subsequent calls will write a coma before the name.
		/// </remarks>
		public void WritePropertyName(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (name.Length == 0)
			{
				throw new ArgumentException("name");
			}
			if (currentObjectHasProperties)
			{
				textWriter.Write(',');
			}
			else
			{
				currentObjectHasProperties = true;
			}
			WriteString(name);
			textWriter.Write(':');
		}

		protected bool IsNullOrEmpty(IJsonSerializable instance)
		{
			if (instance != null)
			{
				emptyObjectDetector.IsEmpty = true;
				instance.Serialize(emptyObjectDetector);
				return emptyObjectDetector.IsEmpty;
			}
			return true;
		}

		protected void WriteString(string value)
		{
			textWriter.Write('"');
			foreach (char c in value)
			{
				switch (c)
				{
				case '\\':
					textWriter.Write("\\\\");
					break;
				case '"':
					textWriter.Write("\\\"");
					break;
				case '\n':
					textWriter.Write("\\n");
					break;
				case '\b':
					textWriter.Write("\\b");
					break;
				case '\f':
					textWriter.Write("\\f");
					break;
				case '\r':
					textWriter.Write("\\r");
					break;
				case '\t':
					textWriter.Write("\\t");
					break;
				default:
					textWriter.Write(c);
					break;
				}
			}
			textWriter.Write('"');
		}
	}
}

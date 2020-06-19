using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// A helper class for implementing properties of telemetry and context classes.
	/// </summary>
	internal static class Property
	{
		public const int MaxDictionaryNameLength = 150;

		public const int MaxValueLength = 65536;

		public const int MaxNameLength = 1024;

		public const int MaxMessageLength = 32768;

		public const int MaxUrlLength = 2048;

		private const RegexOptions SanitizeOptions = RegexOptions.Compiled;

		private static readonly Regex InvalidNameCharacters = new Regex("[^0-9a-zA-Z-._()\\/ ]", RegexOptions.Compiled);

		public static void Set<T>(T property, T value) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			property = value;
		}

		public static void Initialize<T>(T? property, T? value) where T : struct
		{
			if (!property.HasValue)
			{
				property = value;
			}
		}

		public static void Initialize(string property, string value)
		{
			if (string.IsNullOrEmpty(property))
			{
				property = value;
			}
		}

		public static string SanitizeName(this string name)
		{
			return TrimAndTruncate(name, 1024);
		}

		public static string SanitizeValue(this string value)
		{
			return TrimAndTruncate(value, 65536);
		}

		public static string SanitizeMessage(this string message)
		{
			return TrimAndTruncate(message, 32768);
		}

		public static Uri SanitizeUri(this Uri uri)
		{
			if (uri != null)
			{
				string text = uri.ToString();
				if (text.Length > 2048)
				{
					text = text.Substring(0, 2048);
					if (Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out Uri result))
					{
						uri = result;
					}
				}
			}
			return uri;
		}

		public static void SanitizeProperties(this IDictionary<string, string> dictionary)
		{
			if (dictionary != null)
			{
				KeyValuePair<string, string>[] array = dictionary.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<string, string> keyValuePair = array[i];
					dictionary.Remove(keyValuePair.Key);
					string key = SanitizeKey(keyValuePair.Key, dictionary);
					string value = keyValuePair.Value.SanitizeValue();
					dictionary.Add(key, value);
				}
			}
		}

		public static void SanitizeMeasurements(this IDictionary<string, double> dictionary)
		{
			if (dictionary != null)
			{
				KeyValuePair<string, double>[] array = dictionary.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<string, double> keyValuePair = array[i];
					dictionary.Remove(keyValuePair.Key);
					string key = SanitizeKey(keyValuePair.Key, dictionary);
					dictionary.Add(key, keyValuePair.Value);
				}
			}
		}

		private static string TrimAndTruncate(string value, int maxLength)
		{
			if (value != null)
			{
				value = value.Trim();
				value = Truncate(value, maxLength);
			}
			return value;
		}

		private static string Truncate(string value, int maxLength)
		{
			if (value.Length <= maxLength)
			{
				return value;
			}
			return value.Substring(0, maxLength);
		}

		private static string SanitizeKey<TValue>(string key, IDictionary<string, TValue> dictionary)
		{
			string input = TrimAndTruncate(key, 150);
			input = InvalidNameCharacters.Replace(input, "_");
			input = MakeKeyNonEmpty(input);
			return MakeKeyUnique(input, dictionary);
		}

		private static string MakeKeyNonEmpty(string key)
		{
			if (!string.IsNullOrEmpty(key))
			{
				return key;
			}
			return "(required property name is empty)";
		}

		private static string MakeKeyUnique<TValue>(string key, IDictionary<string, TValue> dictionary)
		{
			if (dictionary.ContainsKey(key))
			{
				string str = Truncate(key, 147);
				int num = 1;
				do
				{
					key = str + num.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0');
					num++;
				}
				while (dictionary.ContainsKey(key));
			}
			return key;
		}
	}
}

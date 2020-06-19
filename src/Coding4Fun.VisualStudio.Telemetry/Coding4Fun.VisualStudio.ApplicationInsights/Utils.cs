using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using Coding4Fun.VisualStudio.Telemetry;

namespace Coding4Fun.VisualStudio.ApplicationInsights
{
    /// <summary>
    /// Various utilities.
    /// </summary>
    /// <summary>
    /// Various utilities.
    /// </summary>
    internal static class Utils
	{
		/// <summary>
		/// The relative path to the cache for our application data.
		/// </summary>
		private static readonly string[] RelativeFolderPath = new string[3]
		{
			"Coding4Fun",
			"ApplicationInsights",
			"Cache"
		};

		/// <summary>
		/// Gets the input string as a SHA256 Base64 encoded string.
		/// </summary>
		/// <param name="input">The input to hash.</param>
		/// <param name="isCaseSensitive">If set to <c>false</c> the function will produce the same value for any casing of input.</param>
		/// <returns>The hashed value.</returns>
		public static string GetHashedId(string input, bool isCaseSensitive = false)
		{
			if (input == null)
			{
				return string.Empty;
			}
            
			using (SHA256 sHA = SHA256.Create())
			{
				string s = input;
				if (!isCaseSensitive)
				{
					s = input.ToUpperInvariant();
				}
				return new SoapBase64Binary(sHA.ComputeHash(Encoding.UTF8.GetBytes(s))).ToString();
			}
		}

		/// <summary>
		/// Reads the serialized context from persistent storage, or will create a new context if none exits.
		/// </summary>
		/// <param name="fileName">The file to read from storage.</param>
		/// <returns>The fallback context we will be using.</returns>
		public static TType ReadSerializedContext<TType>(string fileName) where TType : IFallbackContext, new()
		{
			string tempPath = Path.GetTempPath();
			tempPath = RelativeFolderPath.Aggregate(tempPath, Path.Combine);
			if (!Directory.Exists(tempPath))
			{
				try
				{
					Directory.CreateDirectory(tempPath);
				}
				catch (IOException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
				catch (NotSupportedException)
				{
				}
			}
			bool flag = true;
			string path = Path.Combine(tempPath, fileName);
			if (File.Exists(path))
			{
				try
				{
					using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read))
					{
						using (XmlReader reader = XmlReader.Create(input))
						{
							XDocument xDocument = XDocument.Load(reader);
							TType result = new TType();
							if (result.Deserialize(xDocument.Root))
							{
								flag = false;
								return result;
							}
						}
					}
				}
				catch (IOException)
				{
				}
				catch (SecurityException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
				catch (XmlException)
				{
				}
				catch (NotSupportedException)
				{
				}
			}
			if (flag)
			{
				XDocument xDocument2 = new XDocument();
				xDocument2.Add(new XElement(XName.Get(typeof(TType).Name)));
				TType result2 = new TType();
				result2.Initialize();
				result2.Serialize(xDocument2.Root);
				try
				{
					using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
					{
						fileStream.SetLength(0L);
						using (XmlWriter xmlWriter = XmlWriter.Create(fileStream))
						{
							xDocument2.Save(xmlWriter);
							xmlWriter.Flush();
							fileStream.Flush();
						}
						return result2;
					}
				}
				catch (IOException)
				{
				}
				catch (SecurityException)
				{
				}
				catch (UnauthorizedAccessException)
				{
				}
				catch (XmlException)
				{
				}
				catch (NotSupportedException)
				{
				}
			}
			TType result3 = new TType();
			result3.Initialize();
			return result3;
		}

		public static bool IsNullOrWhiteSpace(this string value)
		{
			return value?.All(char.IsWhiteSpace) ?? true;
		}

		public static void CopyDictionary<TValue>(IDictionary<string, TValue> source, IDictionary<string, TValue> target)
		{
			foreach (KeyValuePair<string, TValue> item in source)
			{
				if (!string.IsNullOrEmpty(item.Key) && !target.ContainsKey(item.Key))
				{
					target[item.Key] = item.Value;
				}
			}
		}

		/// <summary>
		/// Validates the string and if null or empty populates it with '$parameterName is a required field for $telemetryType' value.
		/// </summary>
		/// <returns></returns>
		public static string PopulateRequiredStringValue(string value, string parameterName, string telemetryType)
		{
			if (string.IsNullOrEmpty(value))
			{
				CoreEventSource.Log.PopulateRequiredStringWithValue(parameterName, telemetryType);
				return parameterName + " is a required field for " + telemetryType;
			}
			return value;
		}

		/// <summary>
		/// Returns default Timespan value if not a valid Timespan.
		/// </summary>
		/// <returns></returns>
		public static TimeSpan ValidateDuration(string value)
		{
			if (!TimeSpanEx.TryParse(value, CultureInfo.InvariantCulture, out TimeSpan output))
			{
				CoreEventSource.Log.RequestTelemetryIncorrectDuration();
				return TimeSpan.Zero;
			}
			return output;
		}
	}
}

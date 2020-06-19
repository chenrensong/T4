using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry.JsonHelpers
{
	/// <summary>
	/// Custom JSON converter. It allows to use specified handlers to convert custom objects to the string.
	/// </summary>
	internal sealed class CustomJsonConverter : JsonConverter
	{
		private readonly Dictionary<Type, Func<object, string>> typeConverters = new Dictionary<Type, Func<object, string>>();

		private readonly HashSet<Type> usedConverter = new HashSet<Type>();

		/// <summary>
		/// Gets a value indicating whether current converter can Read.
		/// </summary>
		public override bool CanRead => false;

		/// <summary>
		/// Add new converter by type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="converter"></param>
		public void AddConverter(Type type, Func<object, string> converter)
		{
			typeConverters[type] = converter;
		}

		/// <summary>
		/// Check whether current converter can deal with certain type.
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns></returns>
		public override bool CanConvert(Type objectType)
		{
			return typeConverters.ContainsKey(objectType);
		}

		/// <summary>
		/// Read Json value to the object. Not implemented for this purposes.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="objectType"></param>
		/// <param name="existingValue"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
		}

		/// <summary>
		/// Reset converters usage information.
		/// </summary>
		public void ResetUsageInformation()
		{
			usedConverter.Clear();
		}

		/// <summary>
		/// Check whether specific converter was used
		/// </summary>
		/// <param name="typeOfConverter"></param>
		/// <returns></returns>
		public bool WasConverterUsed(Type typeOfConverter)
		{
			return usedConverter.Contains(typeOfConverter);
		}

		/// <summary>
		/// Write Json representation of the current value.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value != null)
			{
				serializer.Serialize(writer, (object)typeConverters[value.GetType()](value));
				usedConverter.Add(value.GetType());
			}
		}

		public CustomJsonConverter()
		{
		}
	}
}

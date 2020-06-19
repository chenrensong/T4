using Coding4Fun.VisualStudio.Telemetry.JsonHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Abstract base class, serves as a base class for the object creators during
	/// deserialize objects from the Json using Newtonsoft.Json
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class JsonCreationConverter<T> : JsonConverter
	{
		/// <summary>
		/// Can convert input type to the desired one?
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns></returns>
		public override bool CanConvert(Type objectType)
		{
			return typeof(T).IsAssignableFrom(objectType);
		}

		/// <summary>
		/// Read and parse json file.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="objectType"></param>
		/// <param name="existingValue"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			if ((int)reader.TokenType == 11)
			{
				return null;
			}
			JObject val = JObject.Load(reader);
			T val2 = Create(objectType, val);
			serializer.Populate(((JToken)val).CreateReader(), (object)val2);
			return val2;
		}

		/// <summary>
		/// Write json file.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Create an instance of objectType, based properties in the JSON object
		/// </summary>
		/// <param name="objectType">type of object expected</param>
		/// <param name="jsonObject">
		/// contents of JSON object that will be deserialized
		/// </param>
		/// <returns></returns>
		internal abstract T Create(Type objectType, JObject jsonObject);

		/// <summary>
		/// Does expected field exist?
		/// </summary>
		/// <param name="fieldName"></param>
		/// <param name="jsonObject"></param>
		/// <returns></returns>
		protected bool FieldExists(string fieldName, JObject jsonObject)
		{
			return jsonObject.FieldExists(fieldName);
		}

		protected JsonCreationConverter()
		{
		}
	}
}

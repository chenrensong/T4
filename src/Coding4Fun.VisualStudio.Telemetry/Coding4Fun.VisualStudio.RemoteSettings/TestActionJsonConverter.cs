using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class TestActionJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			throw new NotImplementedException("Should only be used on property");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Invalid comparison between Unknown and I4
			if ((int)reader.TokenType == 9)
			{
				return reader.Value as string;
			}
			return ((object)JObject.Load(reader)).ToString();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException("Cannot use this class to write");
		}

		public TestActionJsonConverter()
		{
		}
	}
}

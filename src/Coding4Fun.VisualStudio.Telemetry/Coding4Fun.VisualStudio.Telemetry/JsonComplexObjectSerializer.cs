using Coding4Fun.VisualStudio.Telemetry.JsonHelpers;
using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class JsonComplexObjectSerializer : IComplexObjectSerializer
	{
		private const string SerializationErrorLabel = "Serialization error: ";

		private readonly Lazy<CustomJsonConverter> jsonConverter = new Lazy<CustomJsonConverter>();

		/// <summary>
		/// Add custom converter to convert object of the certain type to the string.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="converter"></param>
		public void SetTypeConverter(Type type, Func<object, string> converter)
		{
			CodeContract.RequiresArgumentNotNull<Func<object, string>>(converter, "converter");
			jsonConverter.Value.AddConverter(type, converter);
		}

		public string Serialize(object obj)
		{
			//IL_0044: Expected O, but got Unknown
			try
			{
				if (jsonConverter.IsValueCreated)
				{
					jsonConverter.Value.ResetUsageInformation();
					return JsonConvert.SerializeObject(obj, (JsonConverter[])(object)new JsonConverter[1]
					{
						jsonConverter.Value
					});
				}
				return JsonConvert.SerializeObject(obj);
			}
			catch (JsonSerializationException val)
			{
				JsonSerializationException val2 = (JsonSerializationException)(object)val;
				throw new ComplexObjectSerializerException(((Exception)(object)val2).Message, (Exception)(object)val2);
			}
		}

		/// <summary>
		/// Check whether converter for the specifc type was called.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool WasConverterUsedForType(Type type)
		{
			if (jsonConverter.IsValueCreated)
			{
				return jsonConverter.Value.WasConverterUsed(type);
			}
			return false;
		}
	}
}

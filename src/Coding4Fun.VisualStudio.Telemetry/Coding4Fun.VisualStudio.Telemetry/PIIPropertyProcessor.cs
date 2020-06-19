using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class PIIPropertyProcessor : IPiiPropertyProcessor
	{
		private class FipsHMACSHA256 : HMACSHA256
        {
			public FipsHMACSHA256(byte[] key)
			{
				//base.HashName = typeof(SHA256CryptoServiceProvider).AssemblyQualifiedName;
				HashSizeValue = 256;
				Key = key;
			}
		}

		private static readonly string Key = "959069c9-9e93-4fa1-bf16-3f8120d7db0c";

		private const string NotHashedPropertySuffix = ".NotHashed";

		private static readonly HashAlgorithm Encrypter = new FipsHMACSHA256(Encoding.UTF8.GetBytes(Key));

		public string BuildRawPropertyName(string propertyName)
		{
			return propertyName + ".NotHashed";
		}

		public bool CanAddRawValue(IEventProcessorContext eventProcessorContext)
		{
			return eventProcessorContext.HostTelemetrySession.CanCollectPrivateInformation;
		}

		public object ConvertToRawValue(object value)
		{
			TelemetryPiiProperty obj = value as TelemetryPiiProperty;
			CodeContract.RequiresArgumentNotNull<TelemetryPiiProperty>(obj, "piiProperty");
			return obj.RawValue;
		}

		public string ConvertToHashedValue(object value)
		{
			TelemetryPiiProperty telemetryPiiProperty = value as TelemetryPiiProperty;
			CodeContract.RequiresArgumentNotNull<TelemetryPiiProperty>(telemetryPiiProperty, "piiProperty");
			return HashPii(telemetryPiiProperty.StringValue);
		}

		public Type TypeOfPiiProperty()
		{
			return typeof(TelemetryPiiProperty);
		}

		private string HashPii(string value)
		{
            var hash = Encrypter.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
		}
	}
}

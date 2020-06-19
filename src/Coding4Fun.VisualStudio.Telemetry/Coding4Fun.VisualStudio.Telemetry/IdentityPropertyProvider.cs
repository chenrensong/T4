using System.Collections.Generic;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class IdentityPropertyProvider : IPropertyProvider
	{
		internal static readonly string HardwareIdPropertyName = "VS.Core.HardwareId";

		/// <summary>
		/// Adds identity properties to telemetry event
		/// </summary>
		/// <param name="sharedProperties"></param>
		/// <param name="telemetryContext"></param>
		public void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
			sharedProperties.Add(new KeyValuePair<string, object>(HardwareIdPropertyName, IdentityInformationProvider.HardwareIdNotObtained));
		}

		/// <summary>
		/// Posts properties to the provided telemetry context.
		/// </summary>
		/// <param name="telemetryContext"></param>
		/// <param name="token"></param>
		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
		}
	}
}

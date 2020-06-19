using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// A telemetry context initializer that will gather device context information.
	/// </summary>
	public class DeviceContextInitializer : IContextInitializer
	{
		/// <summary>
		/// Initializes the given <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		/// <param name="context">The telemetry context to initialize.</param>
		public void Initialize(TelemetryContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			IDeviceContextReader instance = DeviceContextReader.Instance;
			context.Device.Type = instance.GetDeviceType();
			context.Device.Id = instance.GetDeviceUniqueId();
			instance.GetOperatingSystemAsync().ContinueWith(delegate(Task<string> task)
			{
				if (task.IsCompleted)
				{
					context.Device.OperatingSystem = task.Result;
				}
			});
			context.Device.OemName = instance.GetOemName();
			context.Device.Model = instance.GetDeviceModel();
			context.Device.NetworkType = instance.GetNetworkType().ToString(CultureInfo.InvariantCulture);
			instance.GetScreenResolutionAsync().ContinueWith(delegate(Task<string> task)
			{
				if (task.IsCompleted)
				{
					context.Device.ScreenResolution = task.Result;
				}
			});
			context.Device.Language = instance.GetHostSystemLocale();
		}
	}
}

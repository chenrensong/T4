using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// Initializes SDK Properties: SDK Version and SDKMode.
	/// </summary>
	internal sealed class SdkVersionPropertyContextInitializer : IContextInitializer
	{
		private const string SDKVersion = "SDKVersion";

		private string sdkVersion;

		/// <summary>
		/// Adds a telemetry property for the version of SDK.
		/// </summary>
		public void Initialize(TelemetryContext context)
		{
			string text = LazyInitializer.EnsureInitialized(ref sdkVersion, GetAssemblyVersion);
			if (string.IsNullOrEmpty(context.Internal.SdkVersion))
			{
				context.Internal.SdkVersion = text;
			}
		}

		private string GetAssemblyVersion()
		{
			return typeof(SdkVersionPropertyContextInitializer).Assembly.GetCustomAttributes(false).OfType<AssemblyFileVersionAttribute>().First()
				.Version;
		}
	}
}

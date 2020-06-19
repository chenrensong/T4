using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class InternalScopeFilterProvider : ISingleValueScopeFilterProvider<BoolScopeValue>, IScopeFilterProvider
	{
		private readonly TelemetrySession telemetrySession;

		public string Name => "IsInternal";

		public InternalScopeFilterProvider(TelemetrySession telemetrySession)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			this.telemetrySession = telemetrySession;
		}

		/// <summary>
		/// Handles requests for IsInternal
		/// </summary>
		/// <returns>A True <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.BoolScopeValue" /> if user is internal</returns>
		public BoolScopeValue Provide()
		{
			return new BoolScopeValue(telemetrySession.IsUserMicrosoftInternal);
		}
	}
}

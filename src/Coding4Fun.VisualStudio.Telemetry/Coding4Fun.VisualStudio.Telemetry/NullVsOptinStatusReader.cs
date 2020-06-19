namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Implements a 'null' ITelemetryOptinStatusReader that always returns false. On platforms other than
	/// windows we do not have access to global policy that enables opt-in for the user. We will use this
	/// for all non-Windows platforms
	/// </summary>
	internal sealed class NullVsOptinStatusReader : ITelemetryOptinStatusReader
	{
		public bool ReadIsOptedInStatus(string productVersion)
		{
			return false;
		}

		public bool ReadIsOptedInStatus(TelemetrySession session)
		{
			return false;
		}
	}
}

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Diagnostic telemetry class is intended to gather and post identity telemetry.
	/// </summary>
	internal interface IIdentityTelemetry
	{
		/// <summary>
		/// Gets the IdentityTelemetry's IdentityInformationProvider
		/// </summary>
		IIdentityInformationProvider IdentityInformationProvider
		{
			get;
		}

		/// <summary>
		/// Post Identity telemetry. Generates an event with properties and sends it.
		/// </summary>
		/// <param name="telemetrySession"></param>
		void PostIdentityTelemetryWhenSessionInitialized(TelemetrySession telemetrySession);
	}
}

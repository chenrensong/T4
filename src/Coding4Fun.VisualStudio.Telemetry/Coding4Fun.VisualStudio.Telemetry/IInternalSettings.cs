namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IInternalSettings
	{
		/// <summary>
		/// Whether flag is set, that user is forced to be external
		/// </summary>
		/// <returns></returns>
		bool IsForcedUserExternal();

		/// <summary>
		/// Try to get test host name for the test purposes
		/// </summary>
		/// <param name="testHostName"></param>
		/// <returns></returns>
		bool TryGetTestHostName(out string testHostName);

		/// <summary>
		/// Try to get test app id for the test purposes
		/// </summary>
		/// <param name="testAppId"></param>
		/// <returns></returns>
		bool TryGetTestAppId(out uint testAppId);

		/// <summary>
		/// Get internal settings for the channel specified by its ID.
		/// There are 3 states could be:
		/// - explicitly enabled
		/// - explicitly disabled
		/// - undefined (no settings available)
		/// </summary>
		/// <param name="channelId"></param>
		/// <returns></returns>
		ChannelInternalSetting GetChannelSettings(string channelId);

		/// <summary>
		/// Returns the IP Global Config Domain Name
		/// </summary>
		/// <returns></returns>
		string GetIPGlobalConfigDomainName();

		/// <summary>
		/// Check whether telemetry is completely disabled
		/// </summary>
		/// <returns></returns>
		bool IsTelemetryDisabledCompletely();

		/// <summary>
		/// Check whether local logger is enabled
		/// </summary>
		/// <returns></returns>
		bool IsLocalLoggerEnabled();

		/// <summary>
		/// Returns the sample rate for FaultEvents for Watson pipeline. AI pipeline is always sent (100%)
		/// </summary>
		/// <returns></returns>
		int FaultEventWatsonSamplePercent();

		/// <summary>
		/// Returns the maximum # of Watson samples per session
		/// </summary>
		/// <returns></returns>
		int FaultEventMaximumWatsonReportsPerSession();

		/// <summary>
		/// Returns the mininum # of seconds between Watson samples.
		/// </summary>
		/// <returns></returns>
		int FaultEventMinimumSecondsBetweenWatsonReports();
	}
}

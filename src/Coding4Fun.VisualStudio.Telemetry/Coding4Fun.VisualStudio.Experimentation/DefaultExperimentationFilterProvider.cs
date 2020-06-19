using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Telemetry.Services;
using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// This filter provider provides information about some properties.
	/// </summary>
	public sealed class DefaultExperimentationFilterProvider : IExperimentationFilterProvider
	{
		internal const string Unknown = "unknown";

		private readonly TelemetrySession telemetrySession;

		private readonly IProcessInformationProvider processInformation;

		/// <summary>
		/// Provider is required TelemetrySession to get information from
		/// </summary>
		/// <param name="telemetrySession"></param>
		public DefaultExperimentationFilterProvider(TelemetrySession telemetrySession)
			: this(telemetrySession, new ProcessInformationProvider())
		{
		}

		internal DefaultExperimentationFilterProvider(TelemetrySession telemetrySession, IProcessInformationProvider processInformation)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			this.telemetrySession = telemetrySession;
			CodeContract.RequiresArgumentNotNull<IProcessInformationProvider>(processInformation, "processInformation");
			this.processInformation = processInformation;
		}

		/// <summary>
		/// Gets filters value based on the passed enum.
		/// </summary>
		/// <param name="filter"></param>
		/// <returns>Formatted string to attach to the headers</returns>
		public string GetFilterValue(Filters filter)
		{
			switch (filter)
			{
			case Filters.UserId:
				return telemetrySession.UserId.ToString("N");
			case Filters.IsInternal:
				if (!telemetrySession.IsUserMicrosoftInternal)
				{
					return "0";
				}
				return "1";
			case Filters.ApplicationName:
				return processInformation.GetExeName() ?? "unknown";
			case Filters.ApplicationVersion:
			{
				FileVersion processVersionInfo = processInformation.GetProcessVersionInfo();
				string result = "unknown";
				if (processVersionInfo != null)
				{
					result = processVersionInfo.ToString();
				}
				return result;
			}
			default:
				return string.Empty;
			}
		}
	}
}

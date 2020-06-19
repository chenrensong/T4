using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IIdentityInformationProvider
	{
		/// <summary>
		///     Gets a value indicating whether any value has changed since last persisted values
		/// </summary>
		bool AnyValueChanged
		{
			get;
		}

		/// <summary>
		///     Gets a value indicating whether any the persisted HardwareId has been invalidated;
		/// </summary>
		bool PersistedIdWasInvalidated
		{
			get;
		}

		/// <summary>
		///     Gets the invalidation reason;
		/// </summary>
		string PersistedIdInvalidationReason
		{
			get;
		}

		/// <summary>
		///     Gets the persisted selectected MAC address.
		/// </summary>
		string PersistedSelectedMACAddress
		{
			get;
		}

		/// <summary>
		///     Gets the local machines HostName
		/// </summary>
		string MachineName
		{
			get;
		}

		/// <summary>
		///     Gets the local machines DNS Domain.
		/// </summary>
		string DNSDomain
		{
			get;
		}

		/// <summary>
		///     Gets the BIOS Serial Number of the local machine
		/// </summary>
		string BiosSerialNumber
		{
			get;
		}

		/// <summary>
		///     Gets the BIOS Universially Unique Identifier of the local machine
		/// </summary>
		Guid BiosUUID
		{
			get;
		}

		/// <summary>
		///     Gets the Hardware Id if it exists, null if it doesn't.
		/// </summary>
		string HardwareId
		{
			get;
		}

		/// <summary>
		///     Gets the date the hardwareId was created, null if it does not exist.
		/// </summary>
		DateTime? HardwareIdDate
		{
			get;
		}

		/// <summary>
		///     Gets the kind of Error that occured while parsing the BIOS firmware table, which is Success if no error was
		///     encountered.
		/// </summary>
		BiosFirmwareTableParserError BiosInformationError
		{
			get;
		}

		/// <summary>
		///     Gets the Selected MAC Address of the local machine from customized network interface prioritization logic.
		/// </summary>
		string SelectedMACAddress
		{
			get;
		}

		/// <summary>
		///     Gets a list of the local machine's Network interfaces ordered by customized prioritization logic.
		/// </summary>
		List<NetworkInterfaceCardInformation> PrioritizedNetworkInterfaces
		{
			get;
		}

		TelemetryManifestMachineIdentityConfig MachineIdentityConfig
		{
			get;
		}

		event EventHandler<EventArgs> HardwareIdCalculationCompleted;

		/// <summary>
		///     Trigger collection of identity properties and return those values to be sent via telemetry.
		/// </summary>
		/// <returns>Properties to be sent in Identity Telemetry event</returns>
		IEnumerable<KeyValuePair<string, object>> CollectIdentifiers(List<Exception> collectionExceptions);

		/// <summary>
		/// Called TelemetrySession SessionInitialize to report back any faults encountered obtaining the harwareid or persisting it.
		/// </summary>
		void SchedulePostPersistedSharedPropertyAndSendAnyFaults(TelemetrySession telemetrySession, ITelemetryScheduler scheduler);

		void GetHardwareIdWithCalculationCompletedEvent(Action<string> callback);

		/// <summary>
		/// Responsible for triggering collection of Identity data and populating correct value in default context and persisted shared properties.
		/// </summary>
		void Initialize(TelemetryContext telemetryContext, ITelemetryScheduler contextScheduler, TelemetryManifestMachineIdentityConfig machineIdentityConfig);
	}
}

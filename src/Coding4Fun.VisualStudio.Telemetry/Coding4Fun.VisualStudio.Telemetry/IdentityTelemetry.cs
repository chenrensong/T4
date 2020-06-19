using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Identity telemetry class is intended to gather and post experimental identity telemetry.
	/// </summary>
	internal class IdentityTelemetry : IIdentityTelemetry
	{
		public const string EvaluationValuesEventName = "VS/TelemetryApi/Identity/EvaluationValues";

		public static readonly string EvaluationValuesEventFaultName = "VS/TelemetryApi/Identity/EvaluationValues/Fault";

		public static readonly string ChangeDetectedPropertyName = "ChangeDetected";

		internal const string propertyPrefix = "VS.TelemetryApi.Identity.";

		private List<KeyValuePair<string, Exception>> exceptions = new List<KeyValuePair<string, Exception>>();

		private bool SendIdentityValuesEvent => IdentityInformationProvider.MachineIdentityConfig.SendValuesEvent;

		public IIdentityInformationProvider IdentityInformationProvider
		{
			get;
		}

		private ITelemetryScheduler Scheduler
		{
			get;
		}

		private CancellationToken CancellationToken
		{
			get;
		}

		public IdentityTelemetry(IIdentityInformationProvider identityInformationProvider, ITelemetryScheduler scheduler)
		{
			CodeContract.RequiresArgumentNotNull<IIdentityInformationProvider>(identityInformationProvider, "identityInformationProvider");
			Scheduler = (scheduler ?? new TelemetryScheduler());
			IdentityInformationProvider = identityInformationProvider;
		}

		/// <summary>
		/// Post Identity telemetry. Generates an event with properties and sends it.
		/// </summary>
		/// <param name="telemetrySession"></param>
		public void PostIdentityTelemetryWhenSessionInitialized(TelemetrySession telemetrySession)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			if (!telemetrySession.IsSessionCloned)
			{
				Scheduler.Schedule(delegate
				{
					CollectAndSendIdentityEvaluationValuesEvent(telemetrySession, telemetrySession.CancellationToken);
				}, telemetrySession.CancellationToken);
			}
		}

		private IEnumerable<KeyValuePair<string, object>> GetIdentityProperties(CancellationToken cancellationToken)
		{
			List<Exception> collectionExceptions = new List<Exception>();
			foreach (KeyValuePair<string, object> item in IdentityInformationProvider.CollectIdentifiers(collectionExceptions))
			{
				cancellationToken.ThrowIfCancellationRequested();
				yield return new KeyValuePair<string, object>("VS.TelemetryApi.Identity." + item.Key, item.Value);
			}
			foreach (Exception item2 in collectionExceptions)
			{
				exceptions.Add(new KeyValuePair<string, Exception>("CollectIdentifiersException", item2));
			}
			if (IdentityInformationProvider.AnyValueChanged)
			{
				yield return new KeyValuePair<string, object>("VS.TelemetryApi.Identity.ChangeDetected", 1);
			}
			foreach (NetworkInterfaceCardInformation prioritizedNetworkInterface in IdentityInformationProvider.PrioritizedNetworkInterfaces)
			{
				yield return new KeyValuePair<string, object>(string.Format("{0}{1}.{2}", "VS.TelemetryApi.Identity.", "PrioritizedNetworkInterfaces", prioritizedNetworkInterface.SelectionRank), prioritizedNetworkInterface.Serialize());
			}
			if (IdentityInformationProvider.BiosInformationError != 0)
			{
				yield return new KeyValuePair<string, object>("VS.TelemetryApi.Identity.BiosInformationError", IdentityInformationProvider.BiosInformationError);
			}
			if (IdentityInformationProvider.PersistedIdWasInvalidated)
			{
				yield return new KeyValuePair<string, object>("VS.TelemetryApi.Identity.PersistedIdWasInvalidated", IdentityInformationProvider.PersistedIdInvalidationReason);
			}
		}

		private void CollectAndSendIdentityEvaluationValuesEvent(TelemetrySession telemetrySession, CancellationToken cancellationToken)
		{
			try
			{
				KeyValuePair<string, object>[] array = GetIdentityProperties(cancellationToken).ToArray();
				if (!SendIdentityValuesEvent)
				{
					return;
				}
				TelemetryEvent telemetryEvent = new TelemetryEvent("VS/TelemetryApi/Identity/EvaluationValues");
				KeyValuePair<string, object>[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					KeyValuePair<string, object> keyValuePair = array2[i];
					telemetryEvent.Properties.Add(keyValuePair.Key, keyValuePair.Value);
				}
				cancellationToken.ThrowIfCancellationRequested();
				telemetrySession.PostEvent(telemetryEvent);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception exceptionObject)
			{
				telemetrySession.PostFault(EvaluationValuesEventFaultName, "SendIdentityEvaluationValuesEvent", exceptionObject);
			}
			foreach (KeyValuePair<string, Exception> exception in exceptions)
			{
				telemetrySession.PostFault(EvaluationValuesEventFaultName, exception.Key, exception.Value);
			}
		}
	}
}

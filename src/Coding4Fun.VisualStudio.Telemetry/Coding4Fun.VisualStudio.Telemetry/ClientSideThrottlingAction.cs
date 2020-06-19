using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class ClientSideThrottlingAction : IEventProcessorAction, IEventProcessorActionDiagnostics
	{
		private const long BaseThreshold = 100L;

		private const string UnknownValue = "Unknown";

		private readonly HashSet<string> passthroughEvents = new HashSet<string>();

		private readonly HashSet<string> droppedEvents = new HashSet<string>();

		private readonly HashSet<string> noisyWhiteListEvents = new HashSet<string>();

		private long counter;

		private long whitelistCounter;

		private double resetCounter = 10.0;

		private long threshold;

		private DateTimeOffset bucketStartTime;

		/// <summary>
		/// Gets action priority. 0 - highest, Inf - lowest
		/// Priority is a hardcoded property we use to sort actions for executing.
		/// We don't want to change it from manifest.
		/// </summary>
		public int Priority => 1000;

		public ClientSideThrottlingAction(IEnumerable<string> passthroughEvents = null, double resetCounterOverride = 0.0, long thresholdOverride = 0L)
		{
			if (passthroughEvents != null)
			{
				this.passthroughEvents.UnionWith(passthroughEvents);
			}
			if (resetCounterOverride > 0.0)
			{
				resetCounter = resetCounterOverride;
			}
			if (thresholdOverride > 0)
			{
				threshold = thresholdOverride;
			}
			else
			{
				threshold = (long)(100.0 * resetCounter);
			}
		}

		/// <summary>
		/// Execute action on event, using eventProcessorContext as a provider of the necessary information.
		/// Return true if it is allowed to execute next actions.
		/// Return false action forbids the event.
		///
		/// Please note that although we are throttling, the reset actually occurs when the following event
		/// is processed. This means if we sent 1k events under a second, and do not do anything for 10 minutes
		/// and then send an event, the Reset will occur then. This was done to simplify code as this feature
		/// is primarily for events under development.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns>Indicator, whether current action is not explicitely forbid current event</returns>
		public bool Execute(IEventProcessorContext eventProcessorContext)
		{
			CodeContract.RequiresArgumentNotNull<IEventProcessorContext>(eventProcessorContext, "eventProcessorContext");
			TelemetryEvent telemetryEvent = eventProcessorContext.TelemetryEvent;
			DateTimeOffset postTimestamp = telemetryEvent.PostTimestamp;
			if (bucketStartTime == default(DateTimeOffset))
			{
				bucketStartTime = postTimestamp;
			}
			if (eventProcessorContext.ThrottlingAction == ThrottlingAction.DoNotThrottle || (eventProcessorContext.ThrottlingAction != ThrottlingAction.Throttle && passthroughEvents.Contains(telemetryEvent.Name)))
			{
				if (counter < threshold)
				{
					counter++;
				}
				else if (whitelistCounter >= threshold)
				{
					noisyWhiteListEvents.Add(telemetryEvent.Name);
				}
				whitelistCounter++;
				return true;
			}
			if ((postTimestamp - bucketStartTime).TotalSeconds > resetCounter)
			{
				TelemetrySession hostTelemetrySession = eventProcessorContext.HostTelemetrySession;
				Reset(hostTelemetrySession, hostTelemetrySession.EventProcessor.CurrentManifest, postTimestamp);
			}
			if (counter++ >= threshold)
			{
				droppedEvents.Add(telemetryEvent.Name);
				return false;
			}
			return true;
		}

		public void PostDiagnosticInformation(TelemetrySession mainSession, TelemetryManifest newManifest)
		{
			Reset(mainSession, newManifest, default(DateTimeOffset));
		}

		public void AddPassthroughEventName(string eventName)
		{
			CodeContract.RequiresArgumentNotNullAndNotEmpty(eventName, "eventName");
			passthroughEvents.Add(eventName);
		}

		private void Reset(TelemetrySession mainSession, TelemetryManifest newManifest, DateTimeOffset timeToReset)
		{
			if (mainSession != null && (counter > threshold || whitelistCounter > threshold))
			{
				string value = "Unknown";
				TelemetryManifest currentManifest = mainSession.EventProcessor.CurrentManifest;
				if (currentManifest != null)
				{
					value = currentManifest.Version;
				}
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary["VS.TelemetryApi.DynamicTelemetry.Manifest.Version"] = value;
				dictionary["VS.TelemetryApi.DynamicTelemetry.HostName"] = mainSession.HostName;
				dictionary["VS.TelemetryApi.ClientSideThrottling.Threshold"] = threshold;
				dictionary["VS.TelemetryApi.ClientSideThrottling.TimerReset"] = resetCounter;
				dictionary["VS.TelemetryApi.ClientSideThrottling.BucketStart"] = bucketStartTime.UtcDateTime.ToString("MM/dd/yy H:mm:ss.fffffff", CultureInfo.InvariantCulture);
				if (counter > threshold)
				{
					long num = counter - threshold;
					TelemetryEvent telemetryEvent = new TelemetryEvent("VS/TelemetryApi/ClientSideThrottling");
					DictionaryExtensions.AddRange<string, object>(telemetryEvent.Properties, (IDictionary<string, object>)dictionary, true);
					telemetryEvent.Properties["VS.TelemetryApi.ClientSideThrottling.TotalDropped"] = num;
					telemetryEvent.Properties["VS.TelemetryApi.ClientSideThrottling.Events"] = StringExtensions.Join((IEnumerable<string>)droppedEvents, ",");
					mainSession.PostEvent(telemetryEvent);
				}
				if (whitelistCounter > threshold)
				{
					long num2 = whitelistCounter - threshold;
					TelemetryEvent telemetryEvent2 = new TelemetryEvent("VS/TelemetryApi/ClientSideThrottling/NoisyWhitelist");
					DictionaryExtensions.AddRange<string, object>(telemetryEvent2.Properties, (IDictionary<string, object>)dictionary, true);
					telemetryEvent2.Properties["VS.TelemetryApi.ClientSideThrottling.TotalNoise"] = num2;
					telemetryEvent2.Properties["VS.TelemetryApi.ClientSideThrottling.Events"] = StringExtensions.Join((IEnumerable<string>)noisyWhiteListEvents, ",");
					mainSession.PostEvent(telemetryEvent2);
				}
			}
			counter = 0L;
			whitelistCounter = 0L;
			bucketStartTime = timeToReset;
			if (newManifest != null)
			{
				if (newManifest.ThrottlingThreshold > 0)
				{
					threshold = newManifest.ThrottlingThreshold;
				}
				if (newManifest.ThrottlingTimerReset > 0.0)
				{
					resetCounter = newManifest.ThrottlingTimerReset;
				}
			}
			droppedEvents.Clear();
			noisyWhiteListEvents.Clear();
		}
	}
}

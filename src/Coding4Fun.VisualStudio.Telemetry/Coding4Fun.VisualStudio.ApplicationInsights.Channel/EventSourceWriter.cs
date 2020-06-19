using System;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Text.RegularExpressions;
using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
    /// <summary>
    /// Encapsulates logic for sending a telemetry as a Common Schema 2.0 event.
    /// </summary>
    public sealed class EventSourceWriter : IDisposable
	{
		private readonly string instrumentationKey;

		private readonly EventSource eventSource;

		private readonly EventSourceOptions eventSourceOptions;

		private bool disposed;

		/// <summary>
		/// Gets the underlying EventSource (ETW) ID. Exposed for Unit Tests purposes.
		/// </summary>
		internal Guid ProviderId => eventSource.Guid;

		/// <summary>
		/// Gets the underlying EventSource (ETW) Name. Exposed for Unit Tests purposes.
		/// </summary>
		internal string ProviderName => eventSource.Name;

		/// <summary>
		/// Gets the instrumentation key for this writer. Exposed for Unit Tests purposes.
		/// </summary>
		internal string InstrumentationKey => instrumentationKey;

		internal EventSourceWriter(string instrumentationKey, bool developerMode = false)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			this.instrumentationKey = instrumentationKey;
			string str = RemoveInvalidInstrumentationKeyChars(this.instrumentationKey.ToLowerInvariant());
			string str2;
			string text;
			if (developerMode)
			{
				str2 = "Coding4Fun.ApplicationInsights.Dev.";
				text = "{ba84f32b-8af2-5006-f147-5030cdd7f22d}";
			}
			else
			{
				str2 = "Coding4Fun.ApplicationInsights.";
				text = "{0d943590-b235-5bdb-f854-89520f32fc0b}";
			}
			eventSource = (EventSource)(object)new EventSource(str2 + str, (EventSourceSettings)8, new string[2]
			{
				"ETW_GROUP",
				text
			});
			EventSourceOptions val = default(EventSourceOptions);
			val.Keywords=((EventKeywords)562949953421312L);
			eventSourceOptions = val;
		}

		/// <summary>
		/// Releases resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		internal void WriteTelemetry(ITelemetry telemetryItem)
		{
			if (telemetryItem == null)
			{
				CoreEventSource.Log.LogVerbose("telemetryItem param is null in EventSourceWriter.WriteTelemetry()");
			}
			else if (eventSource.IsEnabled())
			{
				if (telemetryItem is EventTelemetry)
				{
					EventTelemetry eventTelemetry = telemetryItem as EventTelemetry;
					WriteEvent("Event", eventTelemetry.Context, eventTelemetry.Data);
				}
				else if (telemetryItem is ExceptionTelemetry)
				{
					ExceptionTelemetry exceptionTelemetry = telemetryItem as ExceptionTelemetry;
					WriteEvent("Exception", exceptionTelemetry.Context, exceptionTelemetry.Data);
				}
				else if (telemetryItem is MetricTelemetry)
				{
					MetricTelemetry metricTelemetry = telemetryItem as MetricTelemetry;
					WriteEvent("Metric", metricTelemetry.Context, metricTelemetry.Data);
				}
				else if (telemetryItem is PageViewTelemetry)
				{
					PageViewTelemetry pageViewTelemetry = telemetryItem as PageViewTelemetry;
					WriteEvent("PageView", pageViewTelemetry.Context, pageViewTelemetry.Data);
				}
				else if (telemetryItem is RemoteDependencyTelemetry)
				{
					RemoteDependencyTelemetry remoteDependencyTelemetry = telemetryItem as RemoteDependencyTelemetry;
					WriteEvent("RemoteDependency", remoteDependencyTelemetry.Context, remoteDependencyTelemetry.Data);
				}
				else if (telemetryItem is RequestTelemetry)
				{
					RequestTelemetry requestTelemetry = telemetryItem as RequestTelemetry;
					WriteEvent("Request", requestTelemetry.Context, requestTelemetry.Data);
				}
				else if (telemetryItem is SessionStateTelemetry)
				{
					SessionStateTelemetry sessionStateTelemetry = telemetryItem as SessionStateTelemetry;
					sessionStateTelemetry.Data.state = (Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.SessionState)sessionStateTelemetry.State;
					WriteEvent("SessionState", sessionStateTelemetry.Context, sessionStateTelemetry.Data);
				}
				else if (telemetryItem is TraceTelemetry)
				{
					TraceTelemetry traceTelemetry = telemetryItem as TraceTelemetry;
					WriteEvent("Message", traceTelemetry.Context, traceTelemetry.Data);
				}
				else
				{
					string message = string.Format(CultureInfo.InvariantCulture, "Unknown telemtry type: {0}", new object[1]
					{
						telemetryItem.GetType()
					});
					CoreEventSource.Log.LogVerbose(message);
				}
			}
		}

		internal void WriteEvent<T>(string eventName, TelemetryContext context, T data)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			eventSource.Write(eventName, eventSourceOptions, new
			{
				PartA_iKey = instrumentationKey,
				PartA_Tags = context.Tags,
				_B = data
			});
		}

		private static string RemoveInvalidInstrumentationKeyChars(string input)
		{
			return new Regex("(?:[^a-z0-9.])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Replace(input, string.Empty);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					eventSource.Dispose();
				}
				disposed = true;
			}
		}
	}
}

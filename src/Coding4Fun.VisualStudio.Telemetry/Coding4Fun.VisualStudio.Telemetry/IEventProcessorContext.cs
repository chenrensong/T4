using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IEventProcessorContext : IDisposable
	{
		bool IsEventDropped
		{
			get;
			set;
		}

		ThrottlingAction ThrottlingAction
		{
			get;
			set;
		}

		TelemetrySession HostTelemetrySession
		{
			get;
		}

		IEventProcessorRouter Router
		{
			get;
		}

		TelemetryEvent TelemetryEvent
		{
			get;
		}

		/// <summary>
		/// Reset ProcessorContext to be able to process new TelemetryEvent
		/// </summary>
		/// <param name="telemetryEvent"></param>
		void InitForNewEvent(TelemetryEvent telemetryEvent);

		/// <summary>
		/// Remove property from the property list of the event and move it to the
		/// excluded properties list
		/// </summary>
		/// <param name="propertyName"></param>
		void ExcludePropertyFromEvent(string propertyName);

		/// <summary>
		/// Move property from the excluded property list to the event properties list
		/// </summary>
		/// <param name="propertyName"></param>
		void IncludePropertyToEvent(string propertyName);

		/// <summary>
		/// One more way to dispose router asynchronously.
		/// In that case it asks channels to flush everything to the Network asynchronously if channel supported such method.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns>Task for wait on</returns>
		Task DisposeAndTransmitAsync(CancellationToken token);
	}
}

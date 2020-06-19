using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Event processor interface is used to implement processor of the incoming events
	/// using dynamic telemetry settings.
	/// </summary>
	internal interface IEventProcessor : IDisposable
	{
		/// <summary>
		/// Process event, using dynamic telemetry settings
		/// </summary>
		/// <param name="telemetryEvent"></param>
		void ProcessEvent(TelemetryEvent telemetryEvent);

		/// <summary>
		/// Add custom action
		/// </summary>
		/// <param name="eventProcessorAction"></param>
		void AddCustomAction(IEventProcessorAction eventProcessorAction);

		/// <summary>
		/// One more way to dispose router asynchronously.
		/// In that case it asks channels to flush everything to the Network asynchronously if channel supported such method.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns>Task for wait on</returns>
		Task DisposeAndTransmitAsync(CancellationToken token);
	}
}

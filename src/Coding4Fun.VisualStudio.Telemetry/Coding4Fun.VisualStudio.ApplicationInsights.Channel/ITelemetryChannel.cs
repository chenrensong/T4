using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Represents a communication channel for sending telemetry to application insights.
	/// </summary>
	public interface ITelemetryChannel : IDisposable
	{
		/// <summary>
		/// Gets or sets a value indicating whether this channel is in developer mode.
		/// </summary>
		bool DeveloperMode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the endpoint address of the channel.
		/// </summary>
		string EndpointAddress
		{
			get;
			set;
		}

		/// <summary>
		/// Sends an instance of ITelemetry through the channel.
		/// </summary>
		void Send(ITelemetry item);

		/// <summary>
		/// Flushes the in-memory buffer.
		/// </summary>
		void Flush();

		/// <summary>
		/// Flushes the in-memory buffer and transmit data asynchronously.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		Task FlushAndTransmitAsync(CancellationToken token);
	}
}

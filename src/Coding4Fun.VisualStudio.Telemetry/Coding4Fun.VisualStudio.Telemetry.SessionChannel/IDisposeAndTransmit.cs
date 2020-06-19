using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Channels which support dispose and transmit functionality.
	/// </summary>
	public interface IDisposeAndTransmit
	{
		/// <summary>
		/// Transmit all internal buffers to the end-point and dispose channel.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns></returns>
		Task DisposeAndTransmitAsync(CancellationToken token);
	}
}

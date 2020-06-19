using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IMACInformationProvider
	{
		event EventHandler<EventArgs> MACAddressHashCalculationCompleted;

		/// <summary>
		/// Runs the external process to get MAC Address (only if necessary).
		/// <param name="onComplete">Function to call with the MAC address when the process completes</param>
		/// </summary>
		void RunProcessIfNecessary(Action<string> onComplete);

		/// <summary>
		/// Gets the hash of the MAC address of the computer.
		/// <returns>Hashed MAC address</returns>
		/// </summary>
		/// <returns></returns>
		string GetMACAddressHash();
	}
}

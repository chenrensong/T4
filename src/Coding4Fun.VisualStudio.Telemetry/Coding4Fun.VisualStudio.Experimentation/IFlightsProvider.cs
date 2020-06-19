using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Experimentation
{
	internal interface IFlightsProvider : IDisposable
	{
		/// <summary>
		/// Gets list of the flights known so far
		/// </summary>
		IEnumerable<string> Flights
		{
			get;
		}

		event EventHandler<FlightsEventArgs> FlightsUpdated;

		/// <summary>
		/// Wait for all asynchronous operations are ready (like reading from file, network, etc)
		/// When operation completes Flights will be up to dated with actual value
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		Task WaitForReady(CancellationToken token);

		/// <summary>
		/// Start working expensive operations (like network calls)
		/// </summary>
		void Start();
	}
}

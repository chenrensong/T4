using System.Diagnostics.CodeAnalysis;

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Implementation of the IRemoteFileReaderFactory which create preset IRemoteFileReader
	/// which uses RemoteControl to download shipping flights from the Azure account.
	/// </summary>
	[ExcludeFromCodeCoverage]
	internal sealed class ShippedFlightsRemoteFileReaderFactory : FlightsRemoteFileReaderFactoryBase
	{
		private const string DefaultPath = "ShippedFlights.json";

		public ShippedFlightsRemoteFileReaderFactory()
			: base("ShippedFlights.json")
		{
		}
	}
}

using System;
using System.IO;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Mono OS Specific Identity Information Provider implementation
	/// </summary>
	internal sealed class MonoIdentityInformationProvider : IdentityInformationProvider
	{
		private static Func<IPersistentPropertyBag> defaultStorage = () => new MachinePropertyBag(DefaultStorageLocation);

		private static string DefaultStorageLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "Application Support", IdentityInformationProvider.DefaultStorageFileName);

		/// <summary>
		/// Gets the BIOS Serial Number of the local machine
		/// </summary>
		public override string BiosSerialNumber => "Not Implemented";

		/// <summary>
		/// Gets the BIOS Universially Unique Identifier of the local machine
		/// </summary>
		public override Guid BiosUUID => Guid.Empty;

		/// <summary>
		/// Gets the kind of Error that occured while parsing the BIOS firmware table, which is Success if no error was encountered.
		/// </summary>
		public override BiosFirmwareTableParserError BiosInformationError => BiosFirmwareTableParserError.Success;

		internal MonoIdentityInformationProvider(Func<IPersistentPropertyBag> store = null)
			: base(store ?? defaultStorage)
		{
		}
	}
}

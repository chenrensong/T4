using System;
using System.IO;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Windows OS Specific Identity Information Provider implementation
	/// </summary>
	internal sealed class WindowsIdentityInformationProvider : IdentityInformationProvider
	{
		private static Func<IPersistentPropertyBag> defaultStorage = () => new MachinePropertyBag(DefaultStorageLocation);

		private static readonly Lazy<BiosInformation> biosInformation = new Lazy<BiosInformation>(InitializeBiosInformation);

		private static string DefaultStorageLocation
		{
			get
			{
				string text = new string[2]
				{
					Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
					Environment.GetEnvironmentVariable("ALLUSERSPROFILE")
				}.FirstOrDefault((string s) => !string.IsNullOrWhiteSpace(s));
				if (text == null)
				{
					return null;
				}
				return Path.Combine(text, IdentityInformationProvider.DefaultStorageFileName);
			}
		}

		/// <summary>
		/// Gets the BIOS Serial Number of the local machine
		/// </summary>
		public override string BiosSerialNumber => biosInformation.Value.SerialNumber;

		/// <summary>
		/// Gets the BIOS Universially Unique Identifier of the local machine
		/// </summary>
		public override Guid BiosUUID => biosInformation.Value.UUID;

		/// <summary>
		/// Gets the kind of Error that occured while parsing the BIOS firmware table, which is Success if no error was encountered.
		/// </summary>
		public override BiosFirmwareTableParserError BiosInformationError => biosInformation.Value.Error;

		internal WindowsIdentityInformationProvider(Func<IPersistentPropertyBag> store = null)
			: base(store ?? defaultStorage)
		{
		}

		private static BiosInformation InitializeBiosInformation()
		{
			string table = WindowsFirmwareInformationProvider.EnumSystemFirmwareTables(NativeMethods.FirmwareTableProviderSignature.RSMB).FirstOrDefault();
			return BiosFirmwareTableParser.ParseBiosFirmwareTable(WindowsFirmwareInformationProvider.GetSystemFirmwareTable(NativeMethods.FirmwareTableProviderSignature.RSMB, table));
		}
	}
}

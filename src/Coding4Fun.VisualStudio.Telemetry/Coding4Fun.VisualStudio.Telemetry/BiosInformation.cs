using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal struct BiosInformation
	{
		public Version SpecVersion;

		public long TableSize;

		public string SerialNumber;

		public Guid UUID;

		public BiosFirmwareTableParserError Error;
	}
}

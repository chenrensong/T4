namespace Coding4Fun.VisualStudio.Telemetry
{
	internal enum BiosFirmwareTableParserError
	{
		Success,
		SpecVersionUnsupported,
		TableTooSmall,
		RequiredSectionNotFound,
		SerialNumberOrdinalIndexOutOfBound
	}
}

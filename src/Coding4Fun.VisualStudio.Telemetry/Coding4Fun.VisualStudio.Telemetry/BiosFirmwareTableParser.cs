using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Class that provides the logic to parse Bios firmware table
	/// To obtain the BIOS firmware table in Windows use the WindowsfirmwareInformationProvider
	/// </summary>
	internal static class BiosFirmwareTableParser
	{
		private const string BiosSerialNumberNotAvailable = "NotAvailable";

		internal const int MinimumTableLength = 8;

		private static Version MinimumSupportedBiosVersion
		{
			get;
		} = new Version("2.1");


		private static Dictionary<int, Func<BinaryReader, BiosInformation, BiosInformation>> SectionProcessorLookup
		{
			get;
		} = new Dictionary<int, Func<BinaryReader, BiosInformation, BiosInformation>>
		{
			{
				1,
				ProcessBiosSystemInformation
			}
		};


		/// <summary>
		/// Parses the supplied BIOS Firmware Table byte array
		/// </summary>
		/// <param name="biosFirmwareTable">The BIOS firmware table</param>
		/// <returns>a BiosInformation object</returns>
		internal static BiosInformation ParseBiosFirmwareTable(byte[] biosFirmwareTable)
		{
			CodeContract.RequiresArgumentNotNull<byte[]>(biosFirmwareTable, "biosFirmwareTable");
			using (MemoryStream biosFirmwareTable2 = new MemoryStream(biosFirmwareTable))
			{
				return ParseBiosFirmwareTable(biosFirmwareTable2);
			}
		}

		private static int GoToNextSectionToParse(BinaryReader br)
		{
			int result = -1;
			while (br.BaseStream.Position != br.BaseStream.Length && !SectionProcessorLookup.ContainsKey(result = br.ReadByte()))
			{
				byte b = br.ReadByte();
				br.BaseStream.Position += b - 2;
				for (int num = 0; num < 2; num = ((br.ReadByte() == 0) ? (num + 1) : 0))
				{
				}
			}
			return result;
		}

		private static IList<string> GetSringsFromStringsSection(BinaryReader br)
		{
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			List<string> list = new List<string>();
			while (num < 2)
			{
				char c = (char)br.ReadByte();
				num = ((c == '\0') ? (num + 1) : 0);
				if (num == 1)
				{
					list.Add(stringBuilder.ToString());
					stringBuilder.Length = 0;
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			return list;
		}

		private static BiosInformation ProcessBiosSystemInformation(BinaryReader br, BiosInformation biosInformation)
		{
			byte b = br.ReadByte();
			long position = br.BaseStream.Position + b - 2;
			br.BaseStream.Position += 5L;
			byte b2 = br.ReadByte();
			biosInformation.UUID = new Guid(br.ReadBytes(16));
			br.BaseStream.Position = position;
			IList<string> sringsFromStringsSection = GetSringsFromStringsSection(br);
			if (b2 == 0)
			{
				biosInformation.SerialNumber = "Not Provided in System Information Section";
			}
			else if (b2 > sringsFromStringsSection.Count)
			{
				biosInformation.Error = BiosFirmwareTableParserError.SerialNumberOrdinalIndexOutOfBound;
			}
			else
			{
				biosInformation.SerialNumber = sringsFromStringsSection[b2 - 1];
			}
			return biosInformation;
		}

		/// <summary>
		/// Parses the supplied BIOS Firmware Table stream
		/// </summary>
		/// <param name="biosFirmwareTable"></param>
		/// <returns>a BiosInformation object</returns>
		internal static BiosInformation ParseBiosFirmwareTable(Stream biosFirmwareTable)
		{
			BiosInformation biosInformation = default(BiosInformation);
			biosInformation.Error = BiosFirmwareTableParserError.Success;
			biosInformation.SerialNumber = "NotAvailable";
			BiosInformation biosInformation2 = biosInformation;
			biosInformation2.TableSize = biosFirmwareTable.Length;
			if (biosFirmwareTable.Length < 8)
			{
				biosInformation2.Error = BiosFirmwareTableParserError.TableTooSmall;
				return biosInformation2;
			}
			using (BinaryReader binaryReader = new BinaryReader(biosFirmwareTable))
			{
				binaryReader.ReadByte();
				biosInformation2.SpecVersion = new Version($"{binaryReader.ReadByte()}.{binaryReader.ReadByte()}");
				if (!(biosInformation2.SpecVersion < MinimumSupportedBiosVersion))
				{
					binaryReader.ReadByte();
					binaryReader.ReadInt32();
					int key;
					if ((key = GoToNextSectionToParse(binaryReader)) > 0)
					{
						if (binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length - 1)
						{
							biosInformation2.Error = BiosFirmwareTableParserError.RequiredSectionNotFound;
						}
						else
						{
							biosInformation2 = SectionProcessorLookup[key](binaryReader, biosInformation2);
						}
					}
					biosInformation = biosInformation2;
					return biosInformation;
				}
				biosInformation2.Error = BiosFirmwareTableParserError.SpecVersionUnsupported;
				biosInformation = biosInformation2;
				return biosInformation;
			}
		}
	}
}

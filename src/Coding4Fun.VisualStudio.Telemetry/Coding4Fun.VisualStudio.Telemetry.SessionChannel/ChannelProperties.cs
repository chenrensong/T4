using System;

namespace Coding4Fun.VisualStudio.Telemetry.SessionChannel
{
	/// <summary>
	/// Each channel has properties
	/// </summary>
	[Flags]
	public enum ChannelProperties
	{
		/// <summary>
		/// No specific settings
		/// </summary>
		None = 0x0,
		/// <summary>
		/// This channel is used to transfer events by default (for ex. ai07)
		/// </summary>
		Default = 0x1,
		/// <summary>
		/// Channel is used for internal users only
		/// </summary>
		InternalOnly = 0x2,
		/// <summary>
		/// For unit test purposes
		/// </summary>
		Test = 0x4,
		/// <summary>
		/// This channel is not supposed to be used during unit tests
		/// because it is real channel
		/// </summary>
		NotForUnitTest = 0x8,
		/// <summary>
		/// Developer's channel is used to collect all events, even those
		/// which are going to be completely suppressed.
		/// </summary>
		DevChannel = 0x10
	}
}

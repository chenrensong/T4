using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Test channel arguments
	/// </summary>
	public sealed class TelemetryTestChannelEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets event which was posted
		/// </summary>
		public TelemetryEvent Event
		{
			get;
			set;
		}

		/// <summary>
		///  tostring
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Event.ToString();
		}
	}
}

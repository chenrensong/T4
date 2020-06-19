using System.Diagnostics;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class MonoProcessCreationTime : IProcessCreationTime
	{
		public long GetProcessCreationTime()
		{
			return Process.GetCurrentProcess().StartTime.ToUniversalTime().Ticks;
		}
	}
}

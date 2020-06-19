using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class WindowsProcessCreationTime : IProcessCreationTime
	{
		public long GetProcessCreationTime()
		{
			long? processCreationTime = NativeMethods.GetProcessCreationTime();
			if (!processCreationTime.HasValue)
			{
				return DateTime.UtcNow.Ticks;
			}
			return processCreationTime.Value;
		}
	}
}

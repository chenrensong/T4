using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class CachedActionResponseTime
	{
		public DateTime CachedTime
		{
			get;
			set;
		} = DateTime.MinValue;


		public TimeSpan MaxWaitTimeSpan
		{
			get;
			set;
		} = TimeSpan.FromDays(14.0);

	}
}

using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class CachedActionCategoryTime
	{
		public DateTime LastSent
		{
			get;
			set;
		} = DateTime.MinValue;


		public TimeSpan WaitTimeSpan
		{
			get;
			set;
		} = TimeSpan.MinValue;

	}
}

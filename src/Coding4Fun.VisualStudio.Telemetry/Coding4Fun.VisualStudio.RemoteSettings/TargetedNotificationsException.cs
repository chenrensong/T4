using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class TargetedNotificationsException : Exception
	{
		public TargetedNotificationsException(string message)
			: base(message)
		{
		}

		public TargetedNotificationsException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}

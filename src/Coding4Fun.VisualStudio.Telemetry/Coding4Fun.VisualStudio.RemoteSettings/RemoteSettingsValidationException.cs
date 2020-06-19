using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class RemoteSettingsValidationException : Exception
	{
		public RemoteSettingsValidationException(string description)
			: base(description)
		{
		}
	}
}

using Microsoft.Win32;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class MonoRegistryPropertyBag : RegistryPropertyBag
	{
		public MonoRegistryPropertyBag(string processName)
			: base(processName)
		{
		}

		protected override void SetAccessControl(RegistryKey key)
		{
		}
	}
}

using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class MacMACInformationProvider : IMACInformationProvider
	{
		private const string MacInformationProviderVersionKey = "mac.info.provider.version";

		private const string MacInformationProviderVersion = "1";

		private readonly IPersistentPropertyBag persistentStorage;

		private readonly ILegacyApi legacyApi;

		private readonly Lazy<string> persistedMAC;

		public event EventHandler<EventArgs> MACAddressHashCalculationCompleted;

		public MacMACInformationProvider(IPersistentPropertyBag persistentStorage, ILegacyApi legacyApi)
		{
			CodeContract.RequiresArgumentNotNull<IPersistentPropertyBag>(persistentStorage, "persistentStorage");
			CodeContract.RequiresArgumentNotNull<ILegacyApi>(legacyApi, "legacyApi");
			this.persistentStorage = persistentStorage;
			this.legacyApi = legacyApi;
			persistedMAC = new Lazy<string>(() => CalculateMACAddressHash(), LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public string GetMACAddressHash()
		{
			return persistedMAC.Value;
		}

		public void RunProcessIfNecessary(Action<string> onComplete)
		{
		}

		private string CalculateMACAddressHash()
		{
			string persistedMacHash = GetPersistedMacHash();
			if (persistedMacHash != null)
			{
				OnMACAddressHashCalculationCompletedEvent(EventArgs.Empty);
				return persistedMacHash;
			}
			try
			{
				if (MacHardwareIdentification.TryGetFirstPrimaryMacAddress(out string macAddress))
				{
					string text = MACInformationProvider.HashMACAddress(macAddress);
					persistentStorage.SetProperty(MACInformationProvider.MacAddressKey, text);
					persistentStorage.SetProperty("mac.info.provider.version", "1");
					OnMACAddressHashCalculationCompletedEvent(EventArgs.Empty);
					return text;
				}
			}
			catch (Exception)
			{
			}
			OnMACAddressHashCalculationCompletedEvent(EventArgs.Empty);
			return MACInformationProvider.ZeroHash;
		}

		private string GetPersistedMacHash()
		{
			object property = persistentStorage.GetProperty("mac.info.provider.version");
			if (property == null || !(property is string) || string.IsNullOrEmpty((string)property))
			{
				legacyApi.SetSharedMachineId(default(Guid));
				return null;
			}
			property = persistentStorage.GetProperty(MACInformationProvider.MacAddressKey);
			if (property != null && property is string)
			{
				string text = (string)property;
				if (Regex.IsMatch(text, MACInformationProvider.PersistRegex))
				{
					return text;
				}
			}
			return null;
		}

		private void OnMACAddressHashCalculationCompletedEvent(EventArgs e)
		{
			this.MACAddressHashCalculationCompleted?.Invoke(this, e);
		}
	}
}

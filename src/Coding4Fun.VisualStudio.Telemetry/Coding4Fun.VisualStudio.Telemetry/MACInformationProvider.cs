using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal abstract class MACInformationProvider : IMACInformationProvider
	{
		internal static string ZeroHash = "0000000000000000000000000000000000000000000000000000000000000000";

		private readonly IProcessTools processTools;

		private readonly IPersistentPropertyBag persistentStorage;

		private readonly Lazy<string> persistedMAC;

		internal static string MacAddressKey = "mac.address";

		private const string MacRegex = "(?:[a-z0-9]{2}[:\\-]){5}[a-z0-9]{2}";

		private const string ZeroRegex = "(?:00[:\\-]){5}00";

		internal static string PersistRegex = "[a-f0-9]{64}";

		private readonly string command;

		private readonly string commandArgs;

		private bool needToRunProcess;

		private object needToRunProcessLock = new object();

		public event EventHandler<EventArgs> MACAddressHashCalculationCompleted;

		protected MACInformationProvider(IProcessTools processTools, IPersistentPropertyBag persistentStorage, string command, string commandArgs)
		{
			CodeContract.RequiresArgumentNotNull<IProcessTools>(processTools, "processTools");
			CodeContract.RequiresArgumentNotNull<IPersistentPropertyBag>(persistentStorage, "persistentStorage");
			this.processTools = processTools;
			this.persistentStorage = persistentStorage;
			this.command = command;
			this.commandArgs = commandArgs;
			persistedMAC = new Lazy<string>(() => CalculateMACAddressHash(), LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public void RunProcessIfNecessary(Action<string> onComplete)
		{
			if (needToRunProcess)
			{
				lock (needToRunProcessLock)
				{
					if (needToRunProcess)
					{
						processTools.RunCommand(command, delegate(string data)
						{
							if (data != null)
							{
								string text = ParseMACAddress(data);
								if (text != null)
								{
									persistentStorage.SetProperty(MacAddressKey, text);
									onComplete(text);
								}
							}
							OnMACAddressHashCalculationCompletedEvent(EventArgs.Empty);
						}, commandArgs);
						needToRunProcess = false;
					}
				}
			}
		}

		public string GetMACAddressHash()
		{
			return persistedMAC.Value;
		}

		private string CalculateMACAddressHash()
		{
			object property = persistentStorage.GetProperty(MacAddressKey);
			if (property != null && property is string)
			{
				string text = (string)property;
				if (Regex.IsMatch(text, PersistRegex))
				{
					OnMACAddressHashCalculationCompletedEvent(EventArgs.Empty);
					return text;
				}
			}
			needToRunProcess = true;
			return ZeroHash;
		}

		private string ParseMACAddress(string data)
		{
			string text = null;
			foreach (Match item in Regex.Matches(data, "(?:[a-z0-9]{2}[:\\-]){5}[a-z0-9]{2}", RegexOptions.IgnoreCase))
			{
				if (!Regex.IsMatch(item.Value, "(?:00[:\\-]){5}00"))
				{
					text = item.Value;
					break;
				}
			}
			if (text != null)
			{
				text = HashMACAddress(text);
			}
			return text;
		}

		internal static string HashMACAddress(string macAddress)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(macAddress);
			return BitConverter.ToString(FipsCompliantSha.Sha256.ComputeHash(bytes)).Replace("-", string.Empty).ToLowerInvariant();
		}

		private void OnMACAddressHashCalculationCompletedEvent(EventArgs e)
		{
			this.MACAddressHashCalculationCompleted?.Invoke(this, e);
		}
	}
}

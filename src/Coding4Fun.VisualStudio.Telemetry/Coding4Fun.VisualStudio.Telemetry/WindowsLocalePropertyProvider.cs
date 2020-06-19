using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class WindowsLocalePropertyProvider : IPropertyProvider
	{
		private const string SystemLocaleRegistryPath = "SYSTEM\\CurrentControlSet\\Control\\Nls\\Language";

		private const string SystemLocaleRegistryKey = "Default";

		private readonly Lazy<CultureInfo> systemInfo;

		private readonly IRegistryTools registryTools;

		public WindowsLocalePropertyProvider(IRegistryTools regTools)
		{
			CodeContract.RequiresArgumentNotNull<IRegistryTools>(regTools, "regTools");
			registryTools = regTools;
			systemInfo = new Lazy<CultureInfo>(() => InitializeSystemInformation(), false);
		}

		public void AddSharedProperties(List<KeyValuePair<string, object>> sharedProperties, TelemetryContext telemetryContext)
		{
		}

		public void PostProperties(TelemetryContext telemetryContext, CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				return;
			}
			if (systemInfo.Value != null)
			{
				telemetryContext.PostProperty("VS.Core.Locale.System", systemInfo.Value.EnglishName);
				if (token.IsCancellationRequested)
				{
					return;
				}
			}
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			telemetryContext.PostProperty("VS.Core.Locale.User", currentCulture.EnglishName);
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			telemetryContext.PostProperty("VS.Core.Locale.UserUI", currentUICulture.EnglishName);
		}

		private CultureInfo InitializeSystemInformation()
		{
			object keyValue = registryTools.GetRegistryValueFromLocalMachineRoot("SYSTEM\\CurrentControlSet\\Control\\Nls\\Language", "Default", (object)null);
			if (keyValue != null && keyValue is string)
			{
				CultureInfo[] array = null;
				try
				{
					array = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
				}
				catch (AccessViolationException)
				{
				}
				if (array != null)
				{
					CultureInfo cultureInfo = array.FirstOrDefault((CultureInfo item) => item.LCID.ToString("X4", CultureInfo.InvariantCulture) == (string)keyValue);
					if (cultureInfo != null)
					{
						return cultureInfo;
					}
				}
			}
			return null;
		}
	}
}

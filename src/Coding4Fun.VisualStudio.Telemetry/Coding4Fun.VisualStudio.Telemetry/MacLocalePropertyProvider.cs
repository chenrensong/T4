using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class MacLocalePropertyProvider : IPropertyProvider
	{
		private readonly Lazy<CultureInfo> systemInfo;

		public MacLocalePropertyProvider()
		{
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
			return CultureInfo.CurrentCulture;
		}
	}
}

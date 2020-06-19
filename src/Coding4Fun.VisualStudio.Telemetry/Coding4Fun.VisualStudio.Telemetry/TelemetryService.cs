using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Core Telemetry class.
	/// It has default session object.
	/// </summary>
	[CLSCompliant(false)]
	public static class TelemetryService
	{
		private static object lockDefaultSessionCreation = new object();

		internal static TelemetrySession InternalDefaultSession
		{
			get;
			set;
		}

		/// <summary>
		/// Gets default session
		/// </summary>
		public static TelemetrySession DefaultSession
		{
			get
			{
				if (InternalDefaultSession == null)
				{
					lock (lockDefaultSessionCreation)
					{
						if (InternalDefaultSession == null)
						{
							InternalDefaultSession = TelemetrySession.Create();
						}
					}
				}
				return InternalDefaultSession;
			}
		}

		/// <summary>
		/// Gets the singleton <see cref="P:Coding4Fun.VisualStudio.Telemetry.TelemetryService.AssetService" /> instance.
		/// </summary>
		public static AssetService AssetService => AssetService.Instance;

		/// <summary>
		/// Gets or sets the ETW event source instance to be used from telemetry sessions, events
		/// </summary>
		internal static ITelemetryEtwProvider TelemetryEventSource
		{
			get;
			set;
		}

		/// <summary>
		/// Allow user to change the Default Session, especially for a cloned session
		/// e.g. TelemetryService.SetDefaultSession(new TelemetrySession(clonedSettingsString)
		/// </summary>
		/// <param name="telemetrySession"></param>
		public static void SetDefaultSession(TelemetrySession telemetrySession)
		{
			bool flag = true;
			if (InternalDefaultSession == null)
			{
				lock (lockDefaultSessionCreation)
				{
					if (InternalDefaultSession == null)
					{
						InternalDefaultSession = telemetrySession;
						flag = false;
					}
				}
			}
			if (flag)
			{
				throw new InvalidOperationException("Cannot change default session when already set");
			}
		}

		/// <summary>
		/// Create new default session with specified parameters
		/// </summary>
		/// <param name="appInsightsIKey"></param>
		/// <param name="asimovIKey"></param>
		/// <returns></returns>
		public static TelemetrySession CreateAndGetDefaultSession(string appInsightsIKey, string asimovIKey)
		{
			bool flag = true;
			if (InternalDefaultSession == null)
			{
				lock (lockDefaultSessionCreation)
				{
					if (InternalDefaultSession == null)
					{
						CodeContract.RequiresArgumentNotEmptyOrWhitespace(appInsightsIKey, "appInsightsIKey");
						CodeContract.RequiresArgumentNotEmptyOrWhitespace(asimovIKey, "asimovIKey");
						TelemetrySessionInitializer @default = TelemetrySessionInitializer.Default;
						@default.AppInsightsInstrumentationKey = appInsightsIKey;
						@default.AsimovInstrumentationKey = asimovIKey;
						InternalDefaultSession = TelemetrySession.Create(@default);
						flag = false;
					}
				}
			}
			if (flag)
			{
				throw new InvalidOperationException("Unable to create new default Telemetry Session with provided keys.");
			}
			return InternalDefaultSession;
		}

		/// <summary>
		/// Attach test channel for diagnostics
		/// </summary>
		/// <param name="channel"></param>
		public static void AttachTestChannel(ITelemetryTestChannel channel)
		{
			CodeContract.RequiresArgumentNotNull<ITelemetryTestChannel>(channel, "channel");
			GlobalTelemetryTestChannel.Instance.EventPosted += channel.OnPostEvent;
		}

		/// <summary>
		/// Detach test channel
		/// </summary>
		/// <param name="channel"></param>
		public static void DetachTestChannel(ITelemetryTestChannel channel)
		{
			CodeContract.RequiresArgumentNotNull<ITelemetryTestChannel>(channel, "channel");
			GlobalTelemetryTestChannel.Instance.EventPosted -= channel.OnPostEvent;
		}

		/// <summary>
		/// Initialized the host specific ETW provider instance to be used by the telemetry service
		/// This method must be called before any telemetry APIs are used otherwise default provider will be used and
		/// the subsequent InitializeEtwProvider calls will throw.
		/// </summary>
		/// <param name="provider">Provider instance to be used</param>
		public static void InitializeEtwProvider(ITelemetryEtwProvider provider)
		{
			CodeContract.RequiresArgumentNotNull<ITelemetryEtwProvider>(provider, "provider");
			if (TelemetryEventSource != null)
			{
				throw new InvalidOperationException("Telemetry ETW provider can only be initialized once.");
			}
			TelemetryEventSource = provider;
		}

		/// <summary>
		/// Ensures that an ETW provider is initialized
		/// </summary>
		internal static void EnsureEtwProviderInitialized()
		{
			if (TelemetryEventSource == null)
			{
				TelemetryEventSource = new TelemetryNullEtwProvider();
			}
		}
	}
}

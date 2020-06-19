using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Represents settings that are specific to a TelemetrySession.
	/// </summary>
	[DataContract]
	internal sealed class TelemetrySessionSettings
	{
		private const string DefaultHostName = "Default";

		private const uint DefaultAppId = 1000u;

		[DataMember]
		public bool IsOptedIn
		{
			get;
			set;
		}

		[DataMember]
		public string HostName
		{
			get;
			set;
		}

		[DataMember]
		public string AppInsightsInstrumentationKey
		{
			get;
			set;
		}

		[DataMember]
		public string AsimovInstrumentationKey
		{
			get;
			set;
		}

		[DataMember]
		public uint AppId
		{
			get;
			set;
		}

		[DataMember]
		public Guid? UserId
		{
			get;
			set;
		}

		[DataMember(IsRequired = true)]
		public string Id
		{
			get;
			set;
		}

		[DataMember(IsRequired = true)]
		public long ProcessStartTime
		{
			get;
			set;
		}

		[IgnoreDataMember]
		public bool CanOverrideHostName
		{
			get;
			private set;
		}

		[IgnoreDataMember]
		public bool CanOverrideAppId
		{
			get;
			private set;
		}

		public TelemetrySessionSettings(string id, IInternalSettings internalSettings, string appInsightsIKey, string asimovIKey)
			: this(id, internalSettings, appInsightsIKey, asimovIKey, TelemetrySessionInitializer.GetProcessCreationTime())
		{
		}

		public TelemetrySessionSettings(string id, IInternalSettings internalSettings, string appInsightsIKey, string asimovIKey, IProcessCreationTime processCreation)
		{
			CodeContract.RequiresArgumentNotNull<IInternalSettings>(internalSettings, "internalSettings");
			Id = id;
			AppInsightsInstrumentationKey = appInsightsIKey;
			AsimovInstrumentationKey = asimovIKey;
			ProcessStartTime = processCreation.GetProcessCreationTime();
			if (internalSettings.TryGetTestHostName(out string testHostName))
			{
				HostName = testHostName;
			}
			else
			{
				HostName = "Default";
				CanOverrideHostName = true;
			}
			if (internalSettings.TryGetTestAppId(out uint testAppId))
			{
				AppId = testAppId;
				return;
			}
			AppId = 1000u;
			CanOverrideAppId = true;
		}

		/// <summary>
		/// Validate session id
		/// </summary>
		/// <param name="sessionID"></param>
		/// <returns></returns>
		internal static bool IsSessionIdValid(string sessionID)
		{
			return !StringExtensions.IsNullOrWhiteSpace(sessionID);
		}

		internal static TelemetrySessionSettings Parse(string serializedSession)
		{
			if (!IsSerializedSessionValid(serializedSession))
			{
				throw new ArgumentException("Serialized session is not valid", "serializedSession");
			}
			TelemetrySessionSettings telemetrySessionSettings;
			try
			{
				telemetrySessionSettings = Deserialize(serializedSession);
			}
			catch (Exception innerException)
			{
				throw new ArgumentException("Could not deserialize to TelemetrySession object", "serializedSession", innerException);
			}
			if (!IsSessionIdValid(telemetrySessionSettings.Id))
			{
				throw new ArgumentException("Session ID in sessionSettings is not valid", "sessionSettings");
			}
			return telemetrySessionSettings;
		}

		public override bool Equals(object other)
		{
			TelemetrySessionSettings telemetrySessionSettings = other as TelemetrySessionSettings;
			if (telemetrySessionSettings == null)
			{
				return false;
			}
			if (AppId == telemetrySessionSettings.AppId && AppInsightsInstrumentationKey == telemetrySessionSettings.AppInsightsInstrumentationKey && AsimovInstrumentationKey == telemetrySessionSettings.AsimovInstrumentationKey && HostName == telemetrySessionSettings.HostName && Id == telemetrySessionSettings.Id && IsOptedIn == telemetrySessionSettings.IsOptedIn && ProcessStartTime == telemetrySessionSettings.ProcessStartTime)
			{
				Guid? userId = UserId;
				Guid? userId2 = telemetrySessionSettings.UserId;
				if (userId.HasValue != userId2.HasValue)
				{
					return false;
				}
				if (!userId.HasValue)
				{
					return true;
				}
				return userId.GetValueOrDefault() == userId2.GetValueOrDefault();
			}
			return false;
		}

		/// <summary>
		/// Serialize settings to json string. Use manual serialization
		/// to fix bug https://devdiv.visualstudio.com/DevDiv/VS%20IDE%20Telemetry/_workitems/edit/634853
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"{{\"IsOptedIn\":{IsOptedIn.ToString().ToLowerInvariant()},\"HostName\":{StringToJsonValue(HostName)},\"AppInsightsInstrumentationKey\":{StringToJsonValue(AppInsightsInstrumentationKey)},\"AsimovInstrumentationKey\":{StringToJsonValue(AsimovInstrumentationKey)},\"AppId\":{AppId},\"UserId\":{StringToJsonValue(UserId.ToString())},\"Id\":{StringToJsonValue(Id)},\"ProcessStartTime\":{ProcessStartTime}}}";
		}

        public override int GetHashCode()
        {
            unchecked
            {
                return (((((((-1285232646 * -1521134295 + IsOptedIn.GetHashCode()) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(HostName)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AppInsightsInstrumentationKey)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AsimovInstrumentationKey)) * -1521134295 + AppId.GetHashCode()) * -1521134295 + EqualityComparer<Guid?>.Default.GetHashCode(UserId)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Id)) * -1521134295 + ProcessStartTime.GetHashCode();
            }
        }

		private static string StringToJsonValue(string value)
		{
			if (value == null)
			{
				return "null";
			}
			return "\"" + value + "\"";
		}

		/// <summary>
		/// Deserializes the passed in string into a TelemetrySessionSettings object.
		/// This method can throw an exception if the string is not in a format the serializer expects
		/// or the string does not represent a proper TelemetrySessionSettings object.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		private static TelemetrySessionSettings Deserialize(string settings)
		{
			using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(settings)))
			{
				return (TelemetrySessionSettings)new DataContractJsonSerializer(typeof(TelemetrySessionSettings)).ReadObject(stream);
			}
		}

		/// <summary>
		/// Validate serialized session
		/// </summary>
		/// <param name="serializedSession"></param>
		/// <returns></returns>
		private static bool IsSerializedSessionValid(string serializedSession)
		{
			return !StringExtensions.IsNullOrWhiteSpace(serializedSession);
		}
	}
}

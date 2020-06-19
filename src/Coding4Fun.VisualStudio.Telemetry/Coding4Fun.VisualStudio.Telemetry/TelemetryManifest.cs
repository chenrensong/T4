using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifest
	{
		/// <summary>
		/// Format version specifies version manifest file format
		/// that current binaries are able to recognize.
		/// It is primarily used in the TelemetryManifestFileDownloader
		/// to request correct version from the server.
		/// This number is incremented when breaking changes in format are occured.
		/// </summary>
		public const uint FormatVersion = 2u;

		[JsonIgnore]
		private readonly HashSet<string> invalidRules = new HashSet<string>();

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string Version
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "etag")]
		public string Etag
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public long ThrottlingThreshold
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public double ThrottlingTimerReset
		{
			get;
			set;
		}

		[DefaultValue(true)]
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public bool ShouldSendAliasForAllInternalUsers
		{
			get;
			set;
		}

		[JsonIgnore]
		public IEnumerable<string> InvalidRules => invalidRules;

		[JsonIgnore]
		public int InvalidActionCount
		{
			get;
			private set;
		}

		[JsonProperty(PropertyName = "rules")]
		public IEnumerable<TelemetryManifestRule> Rules
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "machineIdentificationConfig")]
		public TelemetryManifestMachineIdentityConfig MachineIdentityConfig
		{
			get;
			set;
		}

		/// <summary>
		/// Build default manifest.
		/// It will contain default channels only
		/// </summary>
		/// <returns></returns>
		public static TelemetryManifest BuildDefaultManifest()
		{
			return new TelemetryManifest
			{
				Version = "default",
				ShouldSendAliasForAllInternalUsers = true
			};
		}

		public IEnumerable<ITelemetryManifestAction> GetActionsForEvent(TelemetryEvent telemetryEvent)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryEvent>(telemetryEvent, "telemetryEvent");
			List<ITelemetryManifestAction> list = new List<ITelemetryManifestAction>();
			foreach (TelemetryManifestRule item in ObjectExtensions.EmptyIfNull<TelemetryManifestRule>(Rules))
			{
				if (item.When.IsEventMatch(telemetryEvent))
				{
					list.AddRange(item.Actions);
				}
			}
			return list;
		}

		/// <summary>
		/// Validate manifest object structure
		/// </summary>
		public void Validate()
		{
			invalidRules.Clear();
			InvalidActionCount = 0;
			if (StringExtensions.IsNullOrWhiteSpace(Version))
			{
				throw new TelemetryManifestValidationException("'version' must be valid non-empty string, represented version");
			}
			Rules = ObjectExtensions.EmptyIfNull<TelemetryManifestRule>(Rules).Where(delegate(TelemetryManifestRule rule)
			{
				try
				{
					rule.Validate();
					return true;
				}
				catch (TelemetryManifestValidationException)
				{
					if (!string.IsNullOrEmpty(rule.Name))
					{
						invalidRules.Add(rule.Name);
					}
				}
				finally
				{
					InvalidActionCount += rule.InvalidActionCount;
				}
				return false;
			}).ToList();
		}

		/// <summary>
		/// Initializes all TelemetryManifestMatchSample.IsSampleActive.
		/// The initialization is based on information from the given session,
		/// such as machine id, session id etc.
		/// </summary>
		/// <param name="session"></param>
		/// <returns></returns>
		internal string CalculateAllSamplings(TelemetrySession session)
		{
			StringBuilder stringBuilder = new StringBuilder("[");
			foreach (TelemetryManifestRule item in ObjectExtensions.EmptyIfNull<TelemetryManifestRule>(Rules))
			{
				stringBuilder.AppendFormat("{0}, ", item.CalculateAllSamplings(session));
			}
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		internal IEnumerable<TelemetryManifestMatchSampling.Path> GetAllSamplings()
		{
			return ObjectExtensions.EmptyIfNull<TelemetryManifestRule>(Rules).SelectMany((TelemetryManifestRule rule) => from sample in rule.GetAllSamplings()
				select new TelemetryManifestMatchSampling.Path(rule, sample));
		}

		/// <summary>
		/// Start validation of the manifest immediately after deserialization
		/// </summary>
		/// <param name="context"></param>
		[OnDeserialized]
		internal void ValidateAfterDeserialization(StreamingContext context)
		{
			Validate();
		}
	}
}

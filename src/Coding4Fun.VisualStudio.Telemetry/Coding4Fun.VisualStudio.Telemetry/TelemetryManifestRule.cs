using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestRule
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string Name
		{
			get;
			set;
		}

		[JsonIgnore]
		public int InvalidActionCount
		{
			get;
			private set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public ITelemetryManifestMatch When
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<ITelemetryManifestAction> Actions
		{
			get;
			set;
		}

		public IEnumerable<TelemetryManifestMatchSampling> GetAllSamplings()
		{
			return When.GetDescendantsAndItself().OfType<TelemetryManifestMatchSampling>();
		}

		public string CalculateAllSamplings(TelemetrySession session)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0}{{", Name);
			foreach (TelemetryManifestMatchSampling allSampling in GetAllSamplings())
			{
				allSampling.CalculateIsSampleActive(this, session);
				stringBuilder.AppendFormat("{0}, ", allSampling.GetFullName(this));
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Validate rule, called automatically by TelemetryManifest object after deserializaing
		/// </summary>
		public virtual void Validate()
		{
			InvalidActionCount = 0;
			if (string.IsNullOrEmpty(Name))
			{
				throw new TelemetryManifestValidationException("'name' must be non-null valid string");
			}
			if (When == null)
			{
				throw new TelemetryManifestValidationException("'when' must be non-null valid matching rule");
			}
			if (Actions == null)
			{
				throw new TelemetryManifestValidationException("'do' must be non-null valid array of the rules");
			}
			When.Validate();
			Actions = Actions.Where(delegate(ITelemetryManifestAction action)
			{
				try
				{
					action.Validate();
					return true;
				}
				catch (TelemetryManifestValidationException)
				{
					InvalidActionCount++;
				}
				return false;
			}).ToList();
			if (!Actions.Any())
			{
				throw new TelemetryManifestValidationException("'do' must have at least 1 action");
			}
		}
	}
}

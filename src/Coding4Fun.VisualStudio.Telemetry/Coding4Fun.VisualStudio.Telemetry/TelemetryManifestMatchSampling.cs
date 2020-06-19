using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestMatchSampling : ITelemetryManifestMatch, ITelemetryEventMatch
	{
		/// <summary>
		/// Contains sampling and correspoding rule.
		/// </summary>
		public sealed class Path
		{
			public readonly TelemetryManifestRule Rule;

			public readonly TelemetryManifestMatchSampling Sampling;

			/// <summary>
			/// Gets full sampling name.
			/// </summary>
			public string FullName => Sampling.GetFullName(Rule);

			public Path(TelemetryManifestRule rule, TelemetryManifestMatchSampling sampling)
			{
				Rule = rule;
				Sampling = sampling;
			}
		}

		[JsonProperty(PropertyName = "flightName")]
		public string Name
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public double Rate
		{
			get;
			set;
		}

		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public IEnumerable<HashInput> Inputs
		{
			get;
			set;
		}

		[JsonIgnore]
		public bool IsSampleActive
		{
			get;
			set;
		}

		[JsonIgnore]
		public bool IsCalculateCalled
		{
			get;
			private set;
		}

		[JsonIgnore]
		public bool IsRateTooLow
		{
			get;
			private set;
		}

		[JsonIgnore]
		public bool IsRateTooHigh
		{
			get;
			private set;
		}

		[JsonIgnore]
		public bool IsInputStringEmpty
		{
			get;
			private set;
		}

		[JsonIgnore]
		public string InputString
		{
			get;
			private set;
		}

		[JsonIgnore]
		public ulong Hash
		{
			get;
			private set;
		}

		public bool IsEventMatch(TelemetryEvent telemetryEvent)
		{
			return IsSampleActive;
		}

		public IEnumerable<ITelemetryManifestMatch> GetChildren()
		{
			return Enumerable.Empty<ITelemetryManifestMatch>();
		}

		public string GetFullName(TelemetryManifestRule rule)
		{
			return rule.Name + (string.IsNullOrEmpty(Name) ? string.Empty : ("." + Name));
		}

		public void CalculateIsSampleActive(TelemetryManifestRule rule, TelemetrySession session)
		{
			IsCalculateCalled = true;
			if (Rate <= 0.0)
			{
				IsSampleActive = false;
				IsRateTooLow = true;
				return;
			}
			if (Rate >= 1.0)
			{
				IsSampleActive = true;
				IsRateTooHigh = true;
				return;
			}
			string text = StringExtensions.Join(from t in new Tuple<HashInput, string>[5]
				{
					Tuple.Create(HashInput.MachineId, session.MachineId.ToString()),
					Tuple.Create(HashInput.UserId, session.UserId.ToString()),
					Tuple.Create(HashInput.RuleId, rule.Name),
					Tuple.Create(HashInput.SamplingId, GetFullName(rule)),
					Tuple.Create(HashInput.SessionId, session.SessionId)
				}
				where Inputs.Contains(t.Item1)
				select t.Item2, ".");
			if (string.IsNullOrEmpty(text))
			{
				IsSampleActive = false;
				IsInputStringEmpty = true;
				return;
			}
			InputString = text;
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			ulong num2 = Hash = BitConverter.ToUInt64(FipsCompliantSha.Sha256.ComputeHash(bytes), 0);
			IsSampleActive = ((double)num2 <= Rate * 1.8446744073709552E+19);
		}

		/// <summary>
		/// Validates only 'this' class but not children. Explicity specify interface so that this method
		/// can only be called an instance typed to the interface and not an implementation type.
		/// </summary>
		void ITelemetryManifestMatch.ValidateItself()
		{
			if (!Inputs.Any())
			{
				throw new TelemetryManifestValidationException("'inputs' must have at least one input");
			}
			if (Rate < 0.0 || Rate > 1.0)
			{
				throw new TelemetryManifestValidationException("'rate' must be a number between 0 and 1");
			}
		}
	}
}

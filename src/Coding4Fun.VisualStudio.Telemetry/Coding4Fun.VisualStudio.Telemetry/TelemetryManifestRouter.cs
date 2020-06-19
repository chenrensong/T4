using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestRouter
	{
		[JsonProperty(/*Could not decode attribute arguments.*/)]
		public string ChannelId
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "args")]
		public ITelemetryManifestRouteArgs Args
		{
			get;
			set;
		}

		/// <summary>
		/// Validate router on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		public virtual void Validate()
		{
			if (StringExtensions.IsNullOrWhiteSpace(ChannelId))
			{
				throw new TelemetryManifestValidationException("'channel' must be valid non-empty channels id");
			}
			if (Args != null)
			{
				Args.Validate();
			}
		}
	}
}

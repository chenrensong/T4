using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal static class TelemetryManifestMatchExtension
	{
		public static IEnumerable<ITelemetryManifestMatch> GetDescendants(this ITelemetryManifestMatch match)
		{
			return (match.GetChildren() ?? Enumerable.Empty<ITelemetryManifestMatch>()).Where((ITelemetryManifestMatch c) => c != null).SelectMany((ITelemetryManifestMatch c) => c.GetDescendantsAndItself());
		}

		public static IEnumerable<ITelemetryManifestMatch> GetDescendantsAndItself(this ITelemetryManifestMatch match)
		{
			return ObjectExtensions.Enumerate<ITelemetryManifestMatch>(match).Concat(match.GetDescendants());
		}

		/// <summary>
		/// Validate rule on post-parsing stage to avoid validate each action everytime during
		/// posting events.
		/// </summary>
		/// <param name="match"></param>
		public static void Validate(this ITelemetryManifestMatch match)
		{
			foreach (ITelemetryManifestMatch item in match.GetDescendantsAndItself())
			{
				item.ValidateItself();
			}
		}
	}
}

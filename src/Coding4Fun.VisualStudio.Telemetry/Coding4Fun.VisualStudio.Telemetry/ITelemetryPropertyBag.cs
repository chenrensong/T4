using System.Collections;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// ITelemetryPropertyBag interface for the generic PropertyBag
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public interface ITelemetryPropertyBag<TValue> : IDictionary<string, TValue>, ICollection<KeyValuePair<string, TValue>>, IEnumerable<KeyValuePair<string, TValue>>, IEnumerable
	{
	}
}

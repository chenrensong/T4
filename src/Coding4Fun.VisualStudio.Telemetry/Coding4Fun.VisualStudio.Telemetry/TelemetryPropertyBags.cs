using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Collections of the property bags
	/// </summary>
	public static class TelemetryPropertyBags
	{
		/// <summary>
		/// Concurrent property bag
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		public class Concurrent<TValue> : ConcurrentDictionary<string, TValue>, ITelemetryPropertyBag<TValue>, IDictionary<string, TValue>, ICollection<KeyValuePair<string, TValue>>, IEnumerable<KeyValuePair<string, TValue>>, IEnumerable
		{
			/// <summary>
			/// Initializer of the concurrent bag
			/// </summary>
			public Concurrent()
				: base((IEqualityComparer<string>)KeyComparer)
			{
			}
		}

		internal sealed class NotConcurrent<TValue> : Dictionary<string, TValue>, ITelemetryPropertyBag<TValue>, IDictionary<string, TValue>, ICollection<KeyValuePair<string, TValue>>, IEnumerable<KeyValuePair<string, TValue>>, IEnumerable
		{
			public NotConcurrent()
				: base((IEqualityComparer<string>)KeyComparer)
			{
			}
		}

		internal static readonly StringComparer KeyComparer = StringComparer.OrdinalIgnoreCase;

		/// <summary>
		/// Check, whether we have properties
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="bag"></param>
		/// <returns></returns>
		public static bool HasProperties<TValue>(this ITelemetryPropertyBag<TValue> bag)
		{
			return bag.Count > 0;
		}
	}
}

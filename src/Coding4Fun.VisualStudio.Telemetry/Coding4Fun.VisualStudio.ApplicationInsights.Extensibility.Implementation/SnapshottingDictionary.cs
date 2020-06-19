using System.Collections;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal class SnapshottingDictionary<TKey, TValue> : SnapshottingCollection<KeyValuePair<TKey, TValue>, IDictionary<TKey, TValue>>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
	{
		public ICollection<TKey> Keys => GetSnapshot().Keys;

		public ICollection<TValue> Values => GetSnapshot().Values;

		public TValue this[TKey key]
		{
			get
			{
				return GetSnapshot()[key];
			}
			set
			{
				lock (Collection)
				{
					Collection[key] = value;
					snapshot = null;
				}
			}
		}

		public SnapshottingDictionary()
			: base((IDictionary<TKey, TValue>)new Dictionary<TKey, TValue>())
		{
		}

		public void Add(TKey key, TValue value)
		{
			lock (Collection)
			{
				Collection.Add(key, value);
				snapshot = null;
			}
		}

		public bool ContainsKey(TKey key)
		{
			return GetSnapshot().ContainsKey(key);
		}

		public bool Remove(TKey key)
		{
			lock (Collection)
			{
				bool num = Collection.Remove(key);
				if (num)
				{
					snapshot = null;
				}
				return num;
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return GetSnapshot().TryGetValue(key, out value);
		}

		protected sealed override IDictionary<TKey, TValue> CreateSnapshot(IDictionary<TKey, TValue> collection)
		{
			return new Dictionary<TKey, TValue>(collection);
		}
	}
}

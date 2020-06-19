using System.Collections;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal abstract class SnapshottingCollection<TItem, TCollection> : ICollection<TItem>, IEnumerable<TItem>, IEnumerable where TCollection : class, ICollection<TItem>
	{
		protected readonly TCollection Collection;

		protected TCollection snapshot;

		public int Count => GetSnapshot().Count;

		public bool IsReadOnly => false;

		protected SnapshottingCollection(TCollection collection)
		{
			Collection = collection;
		}

		public void Add(TItem item)
		{
			lock (Collection)
			{
				Collection.Add(item);
				snapshot = null;
			}
		}

		public void Clear()
		{
			lock (Collection)
			{
				Collection.Clear();
				snapshot = null;
			}
		}

		public bool Contains(TItem item)
		{
			return GetSnapshot().Contains(item);
		}

		public void CopyTo(TItem[] array, int arrayIndex)
		{
			GetSnapshot().CopyTo(array, arrayIndex);
		}

		public bool Remove(TItem item)
		{
			lock (Collection)
			{
				bool num = Collection.Remove(item);
				if (num)
				{
					snapshot = null;
				}
				return num;
			}
		}

		public IEnumerator<TItem> GetEnumerator()
		{
			return GetSnapshot().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		protected abstract TCollection CreateSnapshot(TCollection collection);

		protected TCollection GetSnapshot()
		{
			TCollection val = snapshot;
			if (val == null)
			{
				lock (Collection)
				{
					snapshot = CreateSnapshot(Collection);
					return snapshot;
				}
			}
			return val;
		}
	}
}

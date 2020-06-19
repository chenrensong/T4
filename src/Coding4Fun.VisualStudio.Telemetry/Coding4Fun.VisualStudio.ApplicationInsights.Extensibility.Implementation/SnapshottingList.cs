using System.Collections;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal class SnapshottingList<T> : SnapshottingCollection<T, IList<T>>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
	{
		public T this[int index]
		{
			get
			{
				return GetSnapshot()[index];
			}
			set
			{
				lock (Collection)
				{
					Collection[index] = value;
					snapshot = null;
				}
			}
		}

		public SnapshottingList()
			: base((IList<T>)new List<T>())
		{
		}

		public int IndexOf(T item)
		{
			return GetSnapshot().IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			lock (Collection)
			{
				Collection.Insert(index, item);
				snapshot = null;
			}
		}

		public void RemoveAt(int index)
		{
			lock (Collection)
			{
				Collection.RemoveAt(index);
				snapshot = null;
			}
		}

		protected sealed override IList<T> CreateSnapshot(IList<T> collection)
		{
			return new List<T>(collection);
		}
	}
}

using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// A light fixed size queue. If Enqueue is called and queue's limit has reached the last item will be removed.
	/// This data structure is thread safe.
	/// </summary>
	internal class FixedSizeQueue<T>
	{
		private readonly int maxSize;

		private object queueLockObj = new object();

		private Queue<T> queue = new Queue<T>();

		internal FixedSizeQueue(int maxSize)
		{
			this.maxSize = maxSize;
		}

		internal void Enqueue(T item)
		{
			lock (queueLockObj)
			{
				if (queue.Count == maxSize)
				{
					queue.Dequeue();
				}
				queue.Enqueue(item);
			}
		}

		internal bool Contains(T item)
		{
			lock (queueLockObj)
			{
				return queue.Contains(item);
			}
		}
	}
}

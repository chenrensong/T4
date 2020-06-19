using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Accumulates <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" /> items for efficient transmission.
	/// </summary>
	internal class TelemetryBuffer
	{
		/// <summary>
		/// Delegate that is raised when the buffer is full.
		/// </summary>
		public Action OnFull;

		private const int DefaultCapacity = 500;

		private readonly object lockObj = new object();

		private int capacity = 500;

		private List<ITelemetry> items;

		/// <summary>
		/// Gets or sets the maximum number of telemetry items that can be buffered before transmission.
		/// </summary>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The value is zero or less.</exception>
		public int Capacity
		{
			get
			{
				return capacity;
			}
			set
			{
				if (value < 1)
				{
					capacity = 500;
				}
				else
				{
					capacity = value;
				}
			}
		}

		internal TelemetryBuffer()
		{
			items = new List<ITelemetry>();
		}

		public void Enqueue(ITelemetry item)
		{
			if (item == null)
			{
				CoreEventSource.Log.LogVerbose("item is null in TelemetryBuffer.Enqueue");
			}
			else
			{
				lock (lockObj)
				{
					items.Add(item);
					if (items.Count >= Capacity)
					{
						OnFull?.Invoke();
					}
				}
			}
		}

		public IEnumerable<ITelemetry> Dequeue()
		{
			List<ITelemetry> result = null;
			if (items.Count > 0)
			{
				lock (lockObj)
				{
					if (items.Count <= 0)
					{
						return result;
					}
					result = items;
					items = new List<ITelemetry>();
					return result;
				}
			}
			return result;
		}
	}
}

using System;
using System.Diagnostics;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// A highly-accurate, precise and testable clock.
	/// </summary>
	internal class Clock : IClock
	{
		private static readonly DateTimeOffset InitialTimeStamp = DateTimeOffset.Now;

		private static readonly Stopwatch OffsetStopwatch = Stopwatch.StartNew();

		private static IClock instance = new Clock();

		public static IClock Instance
		{
			get
			{
				return instance;
			}
			protected set
			{
				instance = (value ?? new Clock());
			}
		}

		public DateTimeOffset Time => InitialTimeStamp + OffsetStopwatch.Elapsed;

		protected Clock()
		{
		}
	}
}

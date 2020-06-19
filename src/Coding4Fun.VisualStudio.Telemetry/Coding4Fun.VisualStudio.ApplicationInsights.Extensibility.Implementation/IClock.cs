using System;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal interface IClock
	{
		DateTimeOffset Time
		{
			get;
		}
	}
}

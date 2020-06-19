using System;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	internal interface IPlatformDispatcher
	{
		Task RunAsync(Action action);
	}
}

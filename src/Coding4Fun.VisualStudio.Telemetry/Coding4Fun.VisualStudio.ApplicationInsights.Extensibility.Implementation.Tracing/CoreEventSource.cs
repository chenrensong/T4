using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing
{
	internal sealed class CoreEventSource
	{
		public static readonly ICoreEventSource Log;

		static CoreEventSource()
		{
            //         if (Platform.IsWindows)
            //{
            //	Log = CreateWindowsEventSource();
            //}
            //else
            //{
            //	Log = CreateMonoEventSource();
            //}
            Log = CreateMonoEventSource();
        }

		private static ICoreEventSource CreateWindowsEventSource()
		{
			return WindowsCoreEventSource.Log;
		}

		private static ICoreEventSource CreateMonoEventSource()
		{
			return new MonoEventSource();
		}
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Platform
{
	/// <summary>
	/// CLR-specific logic of <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.IApplicationLifecycle" /> provider.
	/// </summary>
	/// <summary>
	/// Common logic of <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.IApplicationLifecycle" /> provider shared by all platforms.
	/// </summary>
	internal class PlatformApplicationLifecycle : IApplicationLifecycle
	{
		private static IApplicationLifecycle provider;

		public static IApplicationLifecycle Provider
		{
			get
			{
				return LazyInitializer.EnsureInitialized(ref provider, CreateDefaultProvider);
			}
			set
			{
				provider = value;
			}
		}

		public event Action<object, object> Started;

		public event EventHandler<ApplicationStoppingEventArgs> Stopping;

		internal void Initialize()
		{
			AppDomain.CurrentDomain.DomainUnload += delegate
			{
				OnStopping(new ApplicationStoppingEventArgs((Func<Task> function) => function()));
			};
		}

		private static IApplicationLifecycle CreateDefaultProvider()
		{
			PlatformApplicationLifecycle platformApplicationLifecycle = new PlatformApplicationLifecycle();
			platformApplicationLifecycle.Initialize();
			return platformApplicationLifecycle;
		}

		private void OnStarted(object eventArgs)
		{
			this.Started?.Invoke(this, eventArgs);
		}

		private void OnStopping(ApplicationStoppingEventArgs eventArgs)
		{
			this.Stopping?.Invoke(this, eventArgs);
		}
	}
}

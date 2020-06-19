using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Platform;
using System;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// This class represents a platform agnostic management interface to application lifecycle events.
	/// </summary>
	public class ApplicationLifecycle : IApplicationLifecycle
	{
		private static readonly object SyncRoot = new object();

		private static ApplicationLifecycle service;

		/// <summary>
		/// The current platform specific application lifecycle provider. We have this additional level of indirection so that we can
		/// replace the provider on the fly without having to unhook all consumers of the events if provides.
		/// </summary>
		private IApplicationLifecycle provider;

		/// <summary>
		/// Gets the singleton instance for our management object.
		/// </summary>
		public static IApplicationLifecycle Service
		{
			get
			{
				if (service == null)
				{
					lock (SyncRoot)
					{
						if (service == null)
						{
							service = new ApplicationLifecycle
							{
								Provider = PlatformApplicationLifecycle.Provider
							};
						}
					}
				}
				return service;
			}
			internal set
			{
				service = (ApplicationLifecycle)value;
			}
		}

		private IApplicationLifecycle Provider
		{
			set
			{
				if (provider != null)
				{
					provider.Started -= OnStarted;
					provider.Stopping -= OnStopping;
				}
				provider = value;
				if (provider != null)
				{
					provider.Started += OnStarted;
					provider.Stopping += OnStopping;
				}
			}
		}

		/// <summary>
		/// Occurs when a new instance of the application is started or an existing instance is activated.
		/// </summary>
		public event Action<object, object> Started;

		/// <summary>
		/// Occurs when the application is suspending or closing.
		/// </summary>
		public event EventHandler<ApplicationStoppingEventArgs> Stopping;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.ApplicationLifecycle" /> class.
		/// </summary>
		protected ApplicationLifecycle()
		{
		}

		/// <summary>
		/// Initializes the current management interface with a platform specific provider.
		/// </summary>
		public static void SetProvider(IApplicationLifecycle provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("applicationLifecycleProvider");
			}
			((ApplicationLifecycle)Service).Provider = provider;
		}

		private void OnStarted(object sender, object eventArgs)
		{
			this.Started?.Invoke(this, eventArgs);
		}

		private void OnStopping(object sender, ApplicationStoppingEventArgs eventArgs)
		{
			this.Stopping?.Invoke(this, eventArgs);
		}
	}
}

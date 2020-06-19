using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Provide context properties, in this case for the default context, either from Host Process, Library,
	/// OS, Machine, User, or any other property provider
	/// </summary>
	internal sealed class DefaultContextPropertyManager : TelemetryDisposableObject, IContextPropertyManager, IDisposable
	{
		private readonly List<IPropertyProvider> propertyProviders;

		private readonly CancellationTokenSource cancellationTokenSource;

		public DefaultContextPropertyManager(IEnumerable<IPropertyProvider> propertyProviders)
		{
			CodeContract.RequiresArgumentNotNull<IEnumerable<IPropertyProvider>>(propertyProviders, "propertyProviders");
			cancellationTokenSource = new CancellationTokenSource();
			this.propertyProviders = propertyProviders.ToList();
		}

		/// <summary>
		/// Adds additional property providers. Used for unit tests.
		/// </summary>
		/// <param name="propertyProvider"></param>
		public void AddPropertyProvider(IPropertyProvider propertyProvider)
		{
			CodeContract.RequiresArgumentNotNull<IPropertyProvider>(propertyProvider, "propertyProvider");
			propertyProviders.Add(propertyProvider);
		}

		/// <summary>
		/// Adds shared properties to the context.
		/// </summary>
		/// <param name="telemetryContext"></param>
		public void AddDefaultContextProperties(TelemetryContext telemetryContext)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryContext>(telemetryContext, "telemetryContext");
			List<KeyValuePair<string, object>> sharedProperties = new List<KeyValuePair<string, object>>();
			propertyProviders.ForEach(delegate(IPropertyProvider x)
			{
				x.AddSharedProperties(sharedProperties, telemetryContext);
			});
			foreach (KeyValuePair<string, object> item in sharedProperties)
			{
				telemetryContext.SharedProperties[item.Key] = item.Value;
			}
		}

		/// <summary>
		/// Post default context properties on a background thread
		/// </summary>
		/// <param name="telemetryContext"></param>
		public void PostDefaultContextProperties(TelemetryContext telemetryContext)
		{
			CancellationToken token = cancellationTokenSource.Token;
			Task.Run(delegate
			{
				propertyProviders.ForEach(delegate(IPropertyProvider x)
				{
					x.PostProperties(telemetryContext, token);
				});
			}, token);
		}

		/// <summary>
		/// Dispose managed resources implementation
		/// </summary>
		protected override void DisposeManagedResources()
		{
			cancellationTokenSource.Cancel();
		}

		public void AddRealtimeDefaultContextProperties(TelemetryContext telemetryContext)
		{
			CodeContract.RequiresArgumentNotNull<TelemetryContext>(telemetryContext, "telemetryContext");
			List<KeyValuePair<string, Func<object>>> list = new List<KeyValuePair<string, Func<object>>>();
			foreach (IRealtimePropertyProvider item in propertyProviders.OfType<IRealtimePropertyProvider>())
			{
				item.AddRealtimeSharedProperties(list, telemetryContext);
			}
			foreach (KeyValuePair<string, Func<object>> item2 in list)
			{
				telemetryContext.RealtimeSharedProperties[item2.Key] = item2.Value;
			}
		}
	}
}

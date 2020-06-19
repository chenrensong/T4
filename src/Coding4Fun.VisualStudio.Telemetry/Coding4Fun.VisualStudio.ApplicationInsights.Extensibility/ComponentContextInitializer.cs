using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using System;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// A telemetry context initializer that will gather component context information.
	/// </summary>
	public class ComponentContextInitializer : IContextInitializer
	{
		/// <summary>
		/// Initializes the given <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" />.
		/// </summary>
		/// <param name="context">The telemetry context to initialize.</param>
		public void Initialize(TelemetryContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			context.Component.Version = ComponentContextReader.Instance.GetVersion();
		}
	}
}

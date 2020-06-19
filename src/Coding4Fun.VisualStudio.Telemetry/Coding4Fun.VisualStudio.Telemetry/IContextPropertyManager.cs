using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Provide context properties, either from Host Process, Library,
	/// OS, Machine, User, or any other property provider
	/// </summary>
	internal interface IContextPropertyManager : IDisposable
	{
		/// <summary>
		/// Adds additional property providers. Used for unit tests.
		/// </summary>
		/// <param name="propertyProvider"></param>
		void AddPropertyProvider(IPropertyProvider propertyProvider);

		/// <summary>
		/// Adds shared properties to the context.
		/// </summary>
		/// <param name="telemetryContext"></param>
		void AddDefaultContextProperties(TelemetryContext telemetryContext);

		void AddRealtimeDefaultContextProperties(TelemetryContext telemetryContext);

		/// <summary>
		/// Post default context properties on a background thread
		/// </summary>
		/// <param name="telemetryContext"></param>
		void PostDefaultContextProperties(TelemetryContext telemetryContext);
	}
}

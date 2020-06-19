using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// Telemetry type used to track user sessions.
	/// </summary>
	public sealed class SessionStateTelemetry : ITelemetry
	{
		internal const string TelemetryName = "SessionState";

		internal readonly SessionStateData Data;

		private readonly TelemetryContext context;

		/// <summary>
		/// Gets or sets the date and time the session state was recorded.
		/// </summary>
		public DateTimeOffset Timestamp
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.TelemetryContext" /> of the application when the session state was recorded.
		/// </summary>
		public TelemetryContext Context => context;

		/// <summary>
		/// Gets or sets the value that defines absolute order of the telemetry item.
		/// </summary>
		public string Sequence
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the value describing state of the user session.
		/// </summary>
		public SessionState State
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SessionStateTelemetry" /> class.
		/// </summary>
		public SessionStateTelemetry()
		{
			Data = new SessionStateData();
			context = new TelemetryContext(new Dictionary<string, string>(), new Dictionary<string, string>());
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SessionStateTelemetry" /> class with the specified <paramref name="state" />.
		/// </summary>
		/// <param name="state">
		/// A <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.SessionState" /> value indicating state of the user session.
		/// </param>
		public SessionStateTelemetry(SessionState state)
			: this()
		{
			State = state;
		}

		/// <summary>
		/// Sanitizes this telemetry instance to ensure it can be accepted by the Application Insights.
		/// </summary>
		void ITelemetry.Sanitize()
		{
		}
	}
}

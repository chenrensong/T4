using Coding4Fun.VisualStudio.ApplicationInsights.Channel;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// Telemetry type used to track exceptions.
	/// </summary>
	public sealed class ExceptionTelemetry : ITelemetry, ISupportProperties
	{
		internal const string TelemetryName = "Exception";

		internal readonly string BaseType = typeof(ExceptionData).Name;

		internal readonly ExceptionData Data;

		private readonly TelemetryContext context;

		private Exception exception;

		/// <summary>
		/// Gets or sets date and time when telemetry was recorded.
		/// </summary>
		public DateTimeOffset Timestamp
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the value that defines absolute order of the telemetry item.
		/// </summary>
		public string Sequence
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the context associated with the current telemetry item.
		/// </summary>
		public TelemetryContext Context => context;

		/// <summary>
		/// Gets or sets the value indicated where the exception was handled.
		/// </summary>
		public ExceptionHandledAt HandledAt
		{
			get
			{
				return ValidateExceptionHandledAt(Data.handledAt);
			}
			set
			{
				Data.handledAt = value.ToString();
			}
		}

		/// <summary>
		/// Gets or sets the original exception tracked by this <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.ITelemetry" />.
		/// </summary>
		public Exception Exception
		{
			get
			{
				return exception;
			}
			set
			{
				exception = value;
				UpdateExceptions(value);
			}
		}

		/// <summary>
		/// Gets a dictionary of application-defined exception metrics.
		/// </summary>
		public IDictionary<string, double> Metrics => Data.measurements;

		/// <summary>
		/// Gets a dictionary of application-defined property names and values providing additional information about this exception.
		/// </summary>
		public IDictionary<string, string> Properties => Data.properties;

		/// <summary>
		/// Gets or sets Exception severity level.
		/// </summary>
		public SeverityLevel? SeverityLevel
		{
			get
			{
				return Data.severityLevel.TranslateSeverityLevel();
			}
			set
			{
				Data.severityLevel = value.TranslateSeverityLevel();
			}
		}

		internal IList<ExceptionDetails> Exceptions => Data.exceptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.ExceptionTelemetry" /> class with empty properties.
		/// </summary>
		public ExceptionTelemetry()
		{
			Data = new ExceptionData();
			context = new TelemetryContext(Data.properties, new Dictionary<string, string>());
			HandledAt = ExceptionHandledAt.Unhandled;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.ExceptionTelemetry" /> class with empty properties.
		/// </summary>
		/// <param name="exception">Exception instance.</param>
		public ExceptionTelemetry(Exception exception)
			: this()
		{
			if (exception == null)
			{
				exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
			}
			Exception = exception;
		}

		/// <summary>
		/// Sanitizes the properties based on constraints.
		/// </summary>
		void ITelemetry.Sanitize()
		{
			Properties.SanitizeProperties();
			Metrics.SanitizeMeasurements();
		}

		private static void ConvertExceptionTree(Exception exception, ExceptionDetails parentExceptionDetails, List<ExceptionDetails> exceptions)
		{
			if (exception == null)
			{
				exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
			}
			ExceptionDetails exceptionDetails = PlatformSingleton.Current.GetExceptionDetails(exception, parentExceptionDetails);
			exceptions.Add(exceptionDetails);
			AggregateException ex = exception as AggregateException;
			if (ex != null)
			{
				foreach (Exception innerException in ex.InnerExceptions)
				{
					ConvertExceptionTree(innerException, exceptionDetails, exceptions);
				}
			}
			else if (exception.InnerException != null)
			{
				ConvertExceptionTree(exception.InnerException, exceptionDetails, exceptions);
			}
		}

		private void UpdateExceptions(Exception exception)
		{
			List<ExceptionDetails> list = new List<ExceptionDetails>();
			ConvertExceptionTree(exception, null, list);
			if (list.Count > 10)
			{
				InnerExceptionCountExceededException ex = new InnerExceptionCountExceededException(string.Format(CultureInfo.InvariantCulture, "The number of inner exceptions was {0} which is larger than {1}, the maximum number allowed during transmission. All but the first {1} have been dropped.", new object[2]
				{
					list.Count,
					10
				}));
				list.RemoveRange(10, list.Count - 10);
				list.Add(PlatformSingleton.Current.GetExceptionDetails(ex, list[0]));
			}
			Data.exceptions = list;
		}

		private ExceptionHandledAt ValidateExceptionHandledAt(string value)
		{
			ExceptionHandledAt result = ExceptionHandledAt.Unhandled;
			if (Enum.IsDefined(typeof(ExceptionHandledAt), value))
			{
				result = (ExceptionHandledAt)Enum.Parse(typeof(ExceptionHandledAt), value);
			}
			return result;
		}
	}
}

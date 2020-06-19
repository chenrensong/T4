using System.Diagnostics.Tracing;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Additional implementation for ExceptionDetails.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	[EventData]
	internal class ExceptionDetails
	{
		public int id
		{
			get;
			set;
		}

		public int outerId
		{
			get;
			set;
		}

		public string typeName
		{
			get;
			set;
		}

		public string message
		{
			get;
			set;
		}

		public bool hasFullStack
		{
			get;
			set;
		}

		public string stack
		{
			get;
			set;
		}

		public IList<StackFrame> parsedStack
		{
			get;
			set;
		}

		public ExceptionDetails()
			: this("AI.ExceptionDetails", "ExceptionDetails")
		{
		}

		protected ExceptionDetails(string fullName, string name)
		{
			typeName = string.Empty;
			message = string.Empty;
			hasFullStack = true;
			stack = string.Empty;
			parsedStack = new List<StackFrame>();
		}

		/// <summary>
		/// Creates a new instance of ExceptionDetails from a System.Exception and a parent ExceptionDetails.
		/// </summary>
		/// <returns></returns>
		internal static ExceptionDetails CreateWithoutStackInfo(Exception exception, ExceptionDetails parentExceptionDetails)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception");
			}
			ExceptionDetails exceptionDetails = new ExceptionDetails
			{
				id = exception.GetHashCode(),
				typeName = exception.GetType().FullName,
				message = exception.Message
			};
			if (parentExceptionDetails != null)
			{
				exceptionDetails.outerId = parentExceptionDetails.id;
			}
			return exceptionDetails;
		}
	}
}

using System;
using System.Runtime.Serialization;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Exception class for Directive Processors
	/// </summary>
	[Serializable]
	public class DirectiveProcessorException : Exception
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DirectiveProcessorException()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Exception message text.</param>
		public DirectiveProcessorException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Exception message text.</param>
		/// <param name="inner">Inner exception.</param>
		public DirectiveProcessorException(string message, Exception inner)
			: base(message, inner)
		{
		}

		/// <summary>
		/// Serialization constructor.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected DirectiveProcessorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}

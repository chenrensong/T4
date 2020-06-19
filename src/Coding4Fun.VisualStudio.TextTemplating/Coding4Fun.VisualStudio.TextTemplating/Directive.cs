using System.Collections.Generic;
using System.Diagnostics;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Represents a directive in the template file
	/// </summary>
	internal sealed class Directive
	{
		private const string DirectiveProcessorAttributeName = "processor";

		private readonly string directiveName;

		private readonly IDictionary<string, string> parameters;

		private readonly Block block;

		private string directiveProcessorName;

		/// <summary>
		/// The name of the directive
		/// </summary>
		public string DirectiveName
		{
			[DebuggerStepThrough]
			get
			{
				return directiveName;
			}
		}

		/// <summary>
		/// Parameter-Value pairs for the directive
		/// </summary>
		public IDictionary<string, string> Parameters
		{
			[DebuggerStepThrough]
			get
			{
				return parameters;
			}
		}

		/// <summary>
		/// The directive block that this directive came from
		/// </summary>
		public Block Block
		{
			[DebuggerStepThrough]
			get
			{
				return block;
			}
		}

		/// <summary>
		/// The name of the processor for this directive if it is a custom one
		/// </summary>
		public string DirectiveProcessorName
		{
			get
			{
				if (string.IsNullOrEmpty(directiveProcessorName))
				{
					Parameters.TryGetValue("processor", out directiveProcessorName);
				}
				return directiveProcessorName;
			}
		}

		/// <summary>
		/// Make the setter for this property very explicit as it is not a normal operation.
		/// </summary>
		/// <param name="processorName"></param>
		internal void SetDirectiveProcessorName(string processorName)
		{
			directiveProcessorName = processorName;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="directiveName"></param>
		/// <param name="parameters"></param>
		/// <param name="block"></param>
		public Directive(string directiveName, IDictionary<string, string> parameters, Block block)
		{
			this.directiveName = directiveName;
			this.parameters = parameters;
			this.block = block;
		}
	}
}

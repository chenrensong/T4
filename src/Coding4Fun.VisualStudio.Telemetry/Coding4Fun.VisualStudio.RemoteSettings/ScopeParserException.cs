using System;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Scope parser exception class, using in running Scope Strings
	/// to check its correctness.
	/// </summary>
	internal class ScopeParserException : Exception
	{
		public ScopeParserException(string description)
			: base(description)
		{
		}
	}
}

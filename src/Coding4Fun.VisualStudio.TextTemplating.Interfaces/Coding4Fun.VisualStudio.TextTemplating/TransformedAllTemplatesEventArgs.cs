using System;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	public class TransformedAllTemplatesEventArgs : EventArgs
	{
		/// <summary>Whether there were any errors during the run ///.</summary>
		/// <returns>Returns <see cref="T:System.Boolean" />.</returns>
		public bool AnyErrors
		{
			get;
			set;
		}
	}
}

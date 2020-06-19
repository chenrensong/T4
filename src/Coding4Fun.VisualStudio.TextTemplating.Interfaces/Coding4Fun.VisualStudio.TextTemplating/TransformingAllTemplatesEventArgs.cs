using System;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	public class TransformingAllTemplatesEventArgs : EventArgs
	{
		/// <summary>Allow subscriber to cancel the operation</summary>
		/// <returns>Returns <see cref="T:System.Boolean" />.</returns>
		public bool Cancel
		{
			get;
			set;
		}
	}
}

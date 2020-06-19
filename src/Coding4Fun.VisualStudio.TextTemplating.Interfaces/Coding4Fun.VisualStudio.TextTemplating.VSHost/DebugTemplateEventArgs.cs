using System;

namespace Coding4Fun.VisualStudio.TextTemplating.VSHost
{
	public class DebugTemplateEventArgs : EventArgs
	{
		/// <returns>Returns <see cref="T:System.String" />.</returns>
		public string TemplateOutput
		{
			get;
			set;
		}

		/// <returns>Returns <see cref="T:System.Boolean" />.</returns>
		public bool Succeeded
		{
			get;
			set;
		}
	}
}

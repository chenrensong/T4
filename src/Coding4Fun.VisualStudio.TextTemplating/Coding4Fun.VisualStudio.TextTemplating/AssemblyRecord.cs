using System;
using System.Reflection;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Class to manage an assembly and when it was last referenced.
	/// </summary>
	internal class AssemblyRecord
	{
		private readonly Assembly assembly;

		/// <summary>
		/// The assembly for the record
		/// </summary>
		internal Assembly Assembly => assembly;

		/// <summary>
		/// Date the assembly was last referenced
		/// </summary>
		internal DateTime LastUse
		{
			get;
			set;
		}

		internal AssemblyRecord(Assembly assembly)
		{
			this.assembly = assembly;
			LastUse = DateTime.Now;
		}
	}
}

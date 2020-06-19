using System;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Class to monitor the state of shadow copied assemblies for use by hosts
	/// </summary>
	public sealed class ShadowCopyMonitor : MarshalByRefObject
	{
		/// <summary>
		/// Are any of the shadow-copied assemblies obsolete with respect to their on-disk files.
		/// </summary>
		/// <returns>Whether any assemblies are obsolete</returns>
		public bool AreShadowCopiesObsolete => ShadowTimes.AreShadowCopiesObsolete;
	}
}

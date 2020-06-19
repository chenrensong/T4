using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Class created to schedule actions on either caller thread or background thread.
	/// </summary>
	internal interface IAssetServiceThreadScheduler
	{
		/// <summary>
		/// Standalone function. Schedules the action to be run.
		/// </summary>
		/// <param name="action"></param>
		void Schedule(Action action);
	}
}

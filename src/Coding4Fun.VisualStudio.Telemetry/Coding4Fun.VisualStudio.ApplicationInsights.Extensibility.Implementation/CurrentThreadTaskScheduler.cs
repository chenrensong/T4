using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Runs tasks synchronously, on the current thread.
	/// From <a href="http://code.msdn.microsoft.com/Samples-for-Parallel-b4b76364/view/SourceCode" />.
	/// </summary>
	internal sealed class CurrentThreadTaskScheduler : TaskScheduler
	{
		public static readonly TaskScheduler Instance = new CurrentThreadTaskScheduler();

		public override int MaximumConcurrencyLevel => 1;

		protected override void QueueTask(Task task)
		{
			TryExecuteTask(task);
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			return TryExecuteTask(task);
		}

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			return Enumerable.Empty<Task>();
		}
	}
}

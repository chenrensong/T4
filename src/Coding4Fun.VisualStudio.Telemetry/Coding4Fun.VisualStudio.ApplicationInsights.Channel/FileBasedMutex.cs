using System;
using System.IO;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	/// <summary>
	/// Provides a simple synchronization mechanism using a file instead of a Mutex since Mutexes are not supported
	/// on Mono (Mac/Linux)
	/// </summary>
	internal sealed class FileBasedMutex : IDisposable
	{
		private readonly string lockFile;

		private readonly FileSystemWatcher watcher;

		private readonly AutoResetEvent watcherEvent;

		private FileStream stream;

		private bool disposed;

		public FileBasedMutex(string lockFile)
		{
			this.lockFile = lockFile;
			string directoryName = Path.GetDirectoryName(lockFile);
			string fileName = Path.GetFileName(lockFile);
			try
			{
				Directory.CreateDirectory(directoryName);
				watcherEvent = new AutoResetEvent(false);
				watcher = new FileSystemWatcher(directoryName, fileName);
				watcher.Deleted += WatcherDeleted;
				watcher.EnableRaisingEvents = true;
			}
			catch
			{
			}
		}

		public bool AcquireMutex(CancellationToken token)
		{
			if (watcherEvent == null)
			{
				return false;
			}
			watcherEvent.Reset();
			token.Register(delegate
			{
				if (!disposed)
				{
					watcherEvent.Set();
				}
			});
			while (!token.IsCancellationRequested && !disposed)
			{
				if (InternalAcquireMutex(token))
				{
					return true;
				}
				if (!token.IsCancellationRequested)
				{
					watcherEvent.WaitOne();
				}
			}
			return false;
		}

		public void ReleaseLock()
		{
			if (stream != null)
			{
				stream.Unlock(0L, 0L);
				stream.Close();
				stream.Dispose();
				stream = null;
				try
				{
					if (File.Exists(lockFile))
					{
						File.Delete(lockFile);
					}
				}
				catch
				{
				}
			}
		}

		public void Dispose()
		{
			disposed = true;
			ReleaseLock();
			if (watcher != null)
			{
				watcher.Dispose();
			}
			if (watcherEvent != null)
			{
				watcherEvent.Set();
				watcherEvent.Dispose();
			}
		}

		private bool InternalAcquireMutex(CancellationToken token)
		{
			try
			{
				stream = new FileStream(lockFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
				stream.Lock(0L, 0L);
				if (token.IsCancellationRequested || disposed)
				{
					ReleaseLock();
					return false;
				}
				return true;
			}
			catch
			{
				stream = null;
				return false;
			}
		}

		private void WatcherDeleted(object sender, FileSystemEventArgs e)
		{
			if (!disposed)
			{
				watcherEvent.Set();
			}
		}
	}
}

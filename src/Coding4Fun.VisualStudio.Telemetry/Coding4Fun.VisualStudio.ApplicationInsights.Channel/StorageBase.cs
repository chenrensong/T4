using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using Coding4Fun.VisualStudio.LocalLogger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal abstract class StorageBase
	{
		/// <summary>
		/// Peeked transmissions dictionary (maps file name to its full path). Holds all the transmissions that were peeked.
		/// </summary>
		/// <remarks>
		/// Note: The value (=file's full path) is not required in the Storage implementation.
		/// If there was a concurrent Abstract Data Type Set it would have been used instead.
		/// However, since there is no concurrent Set, dictionary is used and the second value is ignored.
		/// </remarks>
		protected IDictionary<string, string> peekedTransmissions;

		/// <summary>
		/// Gets or sets the maximum size of the storage in bytes. When limit is reached, the Enqueue method will drop new transmissions.
		/// </summary>
		internal ulong CapacityInBytes
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the maximum number of files. When limit is reached, the Enqueue method will drop new transmissions.
		/// </summary>
		internal uint MaxFiles
		{
			get;
			set;
		}

		internal abstract string FolderName
		{
			get;
		}

		internal abstract DirectoryInfo StorageFolder
		{
			get;
		}

		internal abstract StorageTransmission Peek();

		internal abstract IEnumerable<StorageTransmission> PeekAll(CancellationToken token);

		internal abstract void Delete(StorageTransmission transmission);

		internal abstract Task EnqueueAsync(Transmission transmission);

		protected void OnPeekedItemDisposed(string fileName)
		{
			try
			{
				if (LocalFileLoggerService.Default.Enabled)
				{
					LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "StorageBase.OnPeekedItemDisposed dispose file {0}", new object[1]
					{
						fileName
					}));
				}
				if (peekedTransmissions.ContainsKey(fileName))
				{
					peekedTransmissions.Remove(fileName);
				}
			}
			catch (Exception ex)
			{
				CoreEventSource.Log.LogVerbose("Failed to remove the item from storage items. Exception: " + ex.ToString());
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", string.Format(CultureInfo.InvariantCulture, "StorageBase.OnPeekedItemDisposed exception dispose file {0}", new object[1]
				{
					fileName
				}));
			}
		}
	}
}

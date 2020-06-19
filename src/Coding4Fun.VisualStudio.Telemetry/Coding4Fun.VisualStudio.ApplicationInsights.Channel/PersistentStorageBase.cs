using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using Coding4Fun.VisualStudio.LocalLogger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Channel
{
	internal abstract class PersistentStorageBase : StorageBase
	{
		/// <summary>
		/// Needs to track files to delete. Use ConcurrentDictionary which is the best option for
		/// tracking set of items in a thread-safe manner.
		/// See: https://stackoverflow.com/questions/18922985/concurrent-hashsett-in-net-framework
		/// </summary>
		private readonly ConcurrentDictionary<string, string> filesToDelete;

		private object peekLockObj = new object();

		private DirectoryInfo storageFolder;

		private int transmissionsDropped;

		private string storageFolderName;

		private bool storageFolderInitialized;

		private object storageFolderLock = new object();

		/// <summary>
		/// Gets the storage's folder name.
		/// </summary>
		internal override string FolderName => storageFolderName;

		/// <summary>
		/// Gets or sets a value indicating whether storage folder was already tried to be created. Only used for UTs.
		/// Once this value is true, StorageFolder will always return null, which mocks scenario that storage's folder
		/// couldn't be created.
		/// </summary>
		internal bool StorageFolderInitialized
		{
			get
			{
				return storageFolderInitialized;
			}
			set
			{
				storageFolderInitialized = value;
			}
		}

		/// <summary>
		/// Gets the storage folder. If storage folder couldn't be created, null will be returned.
		/// </summary>
		internal override DirectoryInfo StorageFolder
		{
			get
			{
				if (!storageFolderInitialized)
				{
					lock (storageFolderLock)
					{
						if (!storageFolderInitialized)
						{
							try
							{
								storageFolder = GetApplicationFolder();
							}
							catch (Exception arg)
							{
								storageFolder = null;
								string message = $"Failed to create storage folder: {arg}";
								CoreEventSource.Log.LogVerbose(message);
							}
							storageFolderInitialized = true;
							string message2 = string.Format("Storage folder: {0}", (storageFolder == null) ? "null" : storageFolder.FullName);
							CoreEventSource.Log.LogVerbose(message2);
						}
					}
				}
				return storageFolder;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistentStorageBase" /> class.
		/// </summary>
		/// <param name="uniqueFolderName">A folder name. Under this folder all the transmissions will be saved.</param>
		internal PersistentStorageBase(string uniqueFolderName)
		{
			peekedTransmissions = new ConcurrentDictionary<string, string>();
			filesToDelete = new ConcurrentDictionary<string, string>();
			storageFolderName = uniqueFolderName;
			if (string.IsNullOrEmpty(uniqueFolderName))
			{
				string applicationIdentity = GetApplicationIdentity();
				storageFolderName = GetSHA1Hash(applicationIdentity);
			}
			base.CapacityInBytes = 10485760uL;
			base.MaxFiles = 5000u;
			Task.Factory.StartNew(DeleteObsoleteFiles).ContinueWith(delegate(Task task)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Storage: Unhandled exception in DeleteObsoleteFiles: {0}", new object[1]
				{
					task.Exception
				});
				CoreEventSource.Log.LogVerbose(message);
			}, TaskContinuationOptions.OnlyOnFaulted);
		}

		/// <summary>
		/// Peek all transmissions at once.
		/// </summary>
		/// <param name="token">Cancellation token</param>
		/// <returns>List of transmissions or empty array</returns>
		internal override IEnumerable<StorageTransmission> PeekAll(CancellationToken token)
		{
			List<StorageTransmission> list = new List<StorageTransmission>();
			lock (peekLockObj)
			{
				foreach (FileInfo filteredFile in GetFilteredFiles())
				{
					token.ThrowIfCancellationRequested();
					StorageTransmission storageTransmission = BuildTransmissionFromFile(filteredFile);
					if (storageTransmission != null)
					{
						list.Add(storageTransmission);
					}
				}
			}
			if (LocalFileLoggerService.Default.Enabled)
			{
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "PersistenceStorageBase.PeekAll peeked {0} transmissions", new object[1]
				{
					list.Count
				}));
			}
			return list;
		}

		/// <summary>
		/// Reads an item from the storage. Order is Last-In-First-Out.
		/// When the Transmission is no longer needed (it was either sent or failed with a non-retriable error) it should be disposed.
		/// </summary>
		/// <returns></returns>
		internal override StorageTransmission Peek()
		{
			lock (peekLockObj)
			{
				foreach (FileInfo filteredFile in GetFilteredFiles())
				{
					StorageTransmission storageTransmission = BuildTransmissionFromFile(filteredFile);
					if (storageTransmission != null)
					{
						if (LocalFileLoggerService.Default.Enabled)
						{
							LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceStorageBase.Peek peeked transmission", new object[1]
							{
								storageTransmission
							}));
						}
						return storageTransmission;
					}
				}
			}
			return null;
		}

		internal override void Delete(StorageTransmission item)
		{
			if (StorageFolder != null)
			{
				filesToDelete[item.FileName] = item.FullFilePath;
				if (LocalFileLoggerService.Default.Enabled)
				{
					LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceStorageBase.Delete try to delete transmission", new object[1]
					{
						item
					}));
				}
				TryRemoveFilesToDelete();
			}
		}

		private void TryRemoveFilesToDelete()
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, string> item in filesToDelete)
			{
				try
				{
					if (LocalFileLoggerService.Default.Enabled)
					{
						LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "PersistenceStorageBase.TryRemoveFilesToDelete try to delete file {0}", new object[1]
						{
							item.Value
						}));
					}
					File.Delete(item.Value);
					list.Add(item.Key);
				}
				catch (Exception ex)
				{
					string message = string.Format(CultureInfo.InvariantCulture, "Failed to delete a file. file: {0} Exception: {1}", new object[2]
					{
						string.IsNullOrEmpty(item.Value) ? "null" : item.Value,
						ex
					});
					CoreEventSource.Log.LogVerbose(message);
					LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", string.Format(CultureInfo.InvariantCulture, "PersistenceStorageBase.TryRemoveFilesToDelete exception happens when deleting file {0}. Exception: {1}", new object[2]
					{
						item.Value ?? "null",
						ex.Message
					}));
				}
			}
			foreach (string item2 in list)
			{
				filesToDelete.TryRemove(item2, out string _);
			}
		}

		internal override async Task EnqueueAsync(Transmission transmission)
		{
			try
			{
				if (StorageFolder != null)
				{
					if (transmission == null)
					{
						CoreEventSource.Log.LogVerbose("transmission is null. EnqueueAsync is skipped");
					}
					else if (IsStorageLimitsReached())
					{
						if (Interlocked.Increment(ref transmissionsDropped) % 100 == 0)
						{
							CoreEventSource.Log.LogVerbose("Total transmissions dropped: " + transmissionsDropped);
						}
					}
					else
					{
						string fileName = BuildFullFileNameWithoutExtension();
						string tempFullFilePath = fileName + ".tmp";
						await SaveTransmissionToFileAsync(transmission, tempFullFilePath).ConfigureAwait(false);
						string text = fileName + ".trn";
						if (LocalFileLoggerService.Default.Enabled)
						{
							LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceStorageBase.EnqueueAsync about to rename {1} to {2}", new object[3]
							{
								transmission,
								Path.GetFileName(tempFullFilePath),
								Path.GetFileName(text)
							}));
						}
						File.Move(tempFullFilePath, text);
					}
				}
			}
			catch (Exception ex)
			{
				CoreEventSource.Log.LogVerbose(string.Format(CultureInfo.InvariantCulture, "EnqueueAsync: Exception: {0}", new object[1]
				{
					ex
				}));
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceStorageBase.EnqueueAsync rename failed with exception: {1}", new object[2]
				{
					transmission,
					ex.Message
				}));
			}
		}

		/// <summary>
		/// Builds transmission from the file object. In the case of success adds
		/// new transmission to the processing queue. No exceptions are thrown.
		/// </summary>
		/// <param name="file">Valid FileInfo object</param>
		/// <returns>StorageTransmission object in case of success or null in case of fail</returns>
		private StorageTransmission BuildTransmissionFromFile(FileInfo file)
		{
			try
			{
				string text = BuildNewFullFileNameWithSameDate(file.Name);
				File.Move(file.FullName, text);
				FileInfo newfile = new FileInfo(text);
				StorageTransmission result = LoadTransmissionFromFileAsync(newfile).ConfigureAwait(false).GetAwaiter().GetResult();
				if (LocalFileLoggerService.Default.Enabled)
				{
					LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceStorageBase.BuildTransmissionFromFile renamed file from {1} to {2}", new object[3]
					{
						result,
						file.Name,
						newfile.Name
					}));
				}
				result.Disposing = delegate
				{
					OnPeekedItemDisposed(newfile.Name);
				};
				peekedTransmissions.Add(newfile.Name, newfile.FullName);
				return result;
			}
			catch (Exception ex)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Failed to load an item from the storage. file: {0} Exception: {1}", new object[2]
				{
					file,
					ex
				});
				CoreEventSource.Log.LogVerbose(message);
			}
			return null;
		}

		/// <summary>
		/// Gets list of the files in the order of Last-In-First-Out. Validate that files are not in the
		/// queue for processing or deleting and that it is possible to delete file.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<FileInfo> GetFilteredFiles()
		{
			IEnumerable<FileInfo> files = GetFiles("*.trn");
			new List<FileInfo>();
			foreach (FileInfo item in files)
			{
				bool flag = false;
				try
				{
					if (!peekedTransmissions.ContainsKey(item.Name) && !filesToDelete.ContainsKey(item.Name) && CanDelete(item))
					{
						flag = true;
					}
				}
				catch (Exception ex)
				{
					string message = string.Format(CultureInfo.InvariantCulture, "Failed to get information about an item from the storage. file: {0} Exception: {1}", new object[2]
					{
						item,
						ex
					});
					CoreEventSource.Log.LogVerbose(message);
				}
				if (flag)
				{
					yield return item;
				}
			}
		}

		/// <summary>
		/// Build file name for the new file without extension.
		/// </summary>
		/// <returns></returns>
		private string BuildFullFileNameWithoutExtension()
		{
			string text = Guid.NewGuid().ToString("N");
			string text2 = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
			return Path.Combine(StorageFolder.FullName, string.Format(CultureInfo.InvariantCulture, "{0}_{1}", new object[2]
			{
				text2,
				text
			}));
		}

		/// <summary>
		/// Generate new file name keeping first part (date) as is.
		/// We need it to not change file order which is used when file is peeked.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private string BuildNewFullFileNameWithSameDate(string name)
		{
			string[] array = name.Split(new char[1]
			{
				'_'
			}, StringSplitOptions.RemoveEmptyEntries);
			if (array.Count() != 2)
			{
				return name;
			}
			string extension = Path.GetExtension(name);
			string text = Guid.NewGuid().ToString("N");
			return Path.Combine(StorageFolder.FullName, string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", new object[3]
			{
				array[0],
				text,
				extension
			}));
		}

		private static async Task SaveTransmissionToFileAsync(Transmission transmission, string fileFullName)
		{
			try
			{
				using (Stream stream = File.OpenWrite(fileFullName))
				{
					if (LocalFileLoggerService.Default.Enabled)
					{
						LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Info, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceStorageBase.SaveTransmissionToFileAsync to file {1}", new object[2]
						{
							transmission,
							Path.GetFileName(fileFullName)
						}));
					}
					await StorageTransmission.SaveAsync(transmission, stream).ConfigureAwait(false);
				}
			}
			catch (UnauthorizedAccessException)
			{
				string message = $"Failed to save transmission to file. UnauthorizedAccessException. File full path: {fileFullName}";
				CoreEventSource.Log.LogVerbose(message);
				LocalFileLoggerService.Default.Log(LocalLoggerSeverity.Error, "Telemetry", string.Format(CultureInfo.InvariantCulture, "Transmission ({0}): PersistenceStorageBase.SaveTransmissionToFileAsync UnauthorizedAccessException", new object[1]
				{
					transmission
				}));
				throw;
			}
		}

		private static async Task<StorageTransmission> LoadTransmissionFromFileAsync(FileInfo file)
		{
			try
			{
				using (Stream stream = file.OpenRead())
				{
					return await StorageTransmission.CreateFromStreamAsync(stream, file.FullName).ConfigureAwait(false);
				}
			}
			catch (Exception arg)
			{
				string message = $"Failed to load transmission from file. File full path: {file.FullName}, Exception: {arg}";
				CoreEventSource.Log.LogVerbose(message);
				throw;
			}
		}

		private static string GetApplicationIdentity()
		{
			string arg = string.Empty;
			try
			{
				arg = WindowsIdentity.GetCurrent().Name;
			}
			catch (Exception arg2)
			{
				CoreEventSource.Log.LogVerbose($"GetApplicationIdentity: Failed to read user identity. Exception: {arg2}");
			}
			string arg3 = string.Empty;
			try
			{
				arg3 = AppDomain.CurrentDomain.BaseDirectory;
			}
			catch (AppDomainUnloadedException arg4)
			{
				CoreEventSource.Log.LogVerbose($"GetApplicationIdentity: Failed to read the domain's base directory. Exception: {arg4}");
			}
			string arg5 = string.Empty;
			try
			{
				arg5 = Process.GetCurrentProcess().ProcessName;
			}
			catch (Exception arg6)
			{
				CoreEventSource.Log.LogVerbose($"GetApplicationIdentity: Failed to read the process name. Exception: {arg6}");
			}
			return $"{arg}@{arg3}{arg5}";
		}

		private static string GetSHA1Hash(string input)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(input);
			try
			{
				byte[] array = new SHA1CryptoServiceProvider().ComputeHash(bytes);
				StringBuilder stringBuilder = new StringBuilder();
				byte[] array2 = array;
				foreach (byte b in array2)
				{
					stringBuilder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
				}
				return stringBuilder.ToString();
			}
			catch (Exception arg)
			{
				CoreEventSource.Log.LogVerbose($"GetSHA1Hash('{input}'): Failed to hash. Change string to Base64. Exception: {arg}");
				return "Storage";
			}
		}

		protected abstract DirectoryInfo GetApplicationFolder();

		private bool IsStorageLimitsReached()
		{
			if (base.MaxFiles == uint.MaxValue && base.CapacityInBytes == ulong.MaxValue)
			{
				return false;
			}
			FileInfo[] files = StorageFolder.GetFiles();
			if (files.Length >= base.MaxFiles)
			{
				return true;
			}
			ulong num = 0uL;
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				try
				{
					ulong length = (ulong)fileInfo.Length;
					num += length;
				}
				catch
				{
				}
			}
			return num >= base.CapacityInBytes;
		}

		/// <summary>
		/// Get files from <see cref="F:Coding4Fun.VisualStudio.ApplicationInsights.Channel.PersistentStorageBase.storageFolder" />.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<FileInfo> GetFiles(string filter)
		{
			IEnumerable<FileInfo> enumerable = new List<FileInfo>();
			try
			{
				if (StorageFolder != null)
				{
					enumerable = StorageFolder.GetFiles(filter, SearchOption.TopDirectoryOnly);
					return enumerable.OrderBy((FileInfo fileInfo) => fileInfo.Name);
				}
				return enumerable;
			}
			catch (Exception arg)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Peek failed while getting files from storage. Exception: " + arg);
				CoreEventSource.Log.LogVerbose(message);
				return enumerable;
			}
		}

		/// <summary>
		/// Enqueue is saving a transmission to a <c>tmp</c> file and after a successful write operation it renames it to a <c>trn</c> file.
		/// A file without a <c>trn</c> extension is ignored by Storage.Peek(), so if a process is taken down before rename happens
		/// it will stay on the disk forever.
		/// This method deletes files with the <c>tmp</c> extension that exists on disk for more than 5 minutes.
		/// </summary>
		private void DeleteObsoleteFiles()
		{
			try
			{
				foreach (FileInfo file in GetFiles("*.tmp"))
				{
					if (DateTime.UtcNow - file.CreationTimeUtc >= TimeSpan.FromMinutes(5.0))
					{
						file.Delete();
					}
				}
			}
			catch (Exception arg)
			{
				CoreEventSource.Log.LogVerbose("Failed to delete tmp files. Exception: " + arg);
			}
		}

		/// <summary>
		/// Simple method to detect if current user has permission to delete the file.
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		protected abstract bool CanDelete(FileInfo fileInfo);
	}
}

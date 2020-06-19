using System;
using System.Collections.Generic;
using System.IO;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class DirectoryReader : IDirectoryReader
	{
		private readonly string path;

		private readonly string directoryName;

		private readonly bool markProcessed;

		private readonly IRemoteSettingsLogger logger;

		public int Priority
		{
			get;
			private set;
		}

		public DirectoryReader(string rootPath, string directoryName, bool markProcessed, int priority, IRemoteSettingsLogger logger)
		{
			path = Path.Combine(rootPath, directoryName);
			this.directoryName = directoryName;
			this.markProcessed = markProcessed;
			Priority = priority;
			this.logger = logger;
		}

		public IEnumerable<DirectoryReaderContext> ReadAllFiles()
		{
			logger.LogInfo("Reading all files from " + path);
			List<DirectoryReaderContext> list = new List<DirectoryReaderContext>();
			try
			{
				if (!Directory.Exists(path))
				{
					return list;
				}
				string[] files = Directory.GetFiles(path, "*.json");
				foreach (string text in files)
				{
					logger.LogVerbose("Opening file for reading: " + text);
					try
					{
						string text2 = text;
						if (markProcessed)
						{
							logger.LogVerbose("Renaming file to .processed: " + text);
							string text3 = text + ".processed";
							File.Move(text, text3);
							text2 = text3;
						}
						DirectoryReaderContext item = new DirectoryReaderContext
						{
							DirectoryName = directoryName,
							FileName = Path.GetFileNameWithoutExtension(text),
							Stream = File.OpenRead(text2)
						};
						list.Add(item);
					}
					catch (Exception exception)
					{
						logger.LogError("Processing file failed", exception);
					}
				}
				return list;
			}
			catch (Exception exception2)
			{
				logger.LogError("Checking directory failed", exception2);
				return list;
			}
		}
	}
}

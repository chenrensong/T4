using System.IO;

namespace Coding4Fun.VisualStudio.LocalLogger
{
	internal sealed class DefaultTextWriterFactory : ITextWriterFactory
	{
		public TextWriter CreateTextWriter(string fullPathName)
		{
			StreamWriter streamWriter = File.CreateText(fullPathName);
			streamWriter.AutoFlush = true;
			return TextWriter.Synchronized(streamWriter);
		}
	}
}

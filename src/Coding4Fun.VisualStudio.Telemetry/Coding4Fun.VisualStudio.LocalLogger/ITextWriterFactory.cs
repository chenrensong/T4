using System.IO;

namespace Coding4Fun.VisualStudio.LocalLogger
{
	internal interface ITextWriterFactory
	{
		TextWriter CreateTextWriter(string fullPathName);
	}
}

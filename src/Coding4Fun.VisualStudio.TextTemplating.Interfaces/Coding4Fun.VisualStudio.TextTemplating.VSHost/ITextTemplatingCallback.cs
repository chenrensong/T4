using System;
using System.Text;

namespace Coding4Fun.VisualStudio.TextTemplating.VSHost
{
	[CLSCompliant(true)]
	public interface ITextTemplatingCallback
	{
		void ErrorCallback(bool warning, string message, int line, int column);

		void SetFileExtension(string extension);

		void SetOutputEncoding(Encoding encoding, bool fromOutputDirective);
	}
}

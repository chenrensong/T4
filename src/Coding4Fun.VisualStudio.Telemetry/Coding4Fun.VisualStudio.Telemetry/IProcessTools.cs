using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IProcessTools
	{
		void RunCommand(string commandName, Action<string> onProcessComplete);

		void RunCommand(string commandName, Action<string> onProcessComplete, string commandArgs);
	}
}

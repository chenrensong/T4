using System;
using System.Diagnostics;
using System.Text;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class ProcessTools : IProcessTools
	{
		public void RunCommand(string commandName, Action<string> onProcessComplete)
		{
			RunCommand(commandName, onProcessComplete, null);
		}

		public void RunCommand(string commandName, Action<string> onProcessComplete, string commandArgs)
		{
			StringBuilder processOutput = new StringBuilder();
			Process process = new Process();
			try
			{
				process.EnableRaisingEvents = true;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.FileName = commandName;
				process.StartInfo.Arguments = (commandArgs ?? string.Empty);
				process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
				{
					processOutput.AppendLine(e.Data);
				};
				process.Exited += delegate
				{
					process.WaitForExit();
					onProcessComplete(processOutput.ToString());
					process.Close();
				};
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}
			catch (Exception)
			{
				process.Close();
			}
		}
	}
}

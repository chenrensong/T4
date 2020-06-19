using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Platform
{
	/// <summary>
	/// The .NET 4.0 and 4.5 implementation of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.IPlatform" /> interface.
	/// </summary>
	internal class PlatformImplementation : IPlatform
	{
		public IDictionary<string, object> GetApplicationSettings()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns contents of the ApplicationInsights.config file in the application directory.
		/// </summary>
		/// <returns></returns>
		public string ReadConfigurationXml()
		{
			string path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "ApplicationInsights.config");
			if (!File.Exists(path))
			{
				path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApplicationInsights.config");
			}
			if (File.Exists(path))
			{
				return File.ReadAllText(path);
			}
			return string.Empty;
		}

		public ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails)
		{
			return ExceptionConverter.ConvertToExceptionDetails(exception, parentExceptionDetails);
		}
	}
}

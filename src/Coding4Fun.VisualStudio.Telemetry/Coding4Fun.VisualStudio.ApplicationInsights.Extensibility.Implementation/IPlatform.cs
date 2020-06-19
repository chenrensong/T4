using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Encapsulates platform-specific functionality required by the API.
	/// </summary>
	/// <remarks>
	/// This type is public to enable mocking on Windows Phone.
	/// </remarks>
	internal interface IPlatform
	{
		/// <summary>
		/// Returns a dictionary that can be used to access per-user/per-application settings shared by all application instances.
		/// </summary>
		/// <returns></returns>
		IDictionary<string, object> GetApplicationSettings();

		/// <summary>
		/// Returns contents of the ApplicationInsights.config file in the application directory.
		/// </summary>
		/// <returns></returns>
		string ReadConfigurationXml();

		/// <summary>
		/// Returns the platform specific <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External.ExceptionDetails" /> object for the given Exception.
		/// </summary>
		/// <returns></returns>
		ExceptionDetails GetExceptionDetails(Exception exception, ExceptionDetails parentExceptionDetails);
	}
}

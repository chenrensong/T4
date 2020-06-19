using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// A telemetry context initializer that will set component context version on the base of BuildInfo.config information.
	/// </summary>
	public class BuildInfoConfigComponentVersionContextInitializer : IContextInitializer
	{
		/// <summary>
		/// The version for this component.
		/// </summary>
		private string version;

		/// <summary>
		/// Initializes a component context version on the base of BuildInfo.config information.
		/// </summary>
		/// <param name="context">The telemetry context to initialize.</param>
		public void Initialize(TelemetryContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (string.IsNullOrEmpty(context.Component.Version))
			{
				string text = LazyInitializer.EnsureInitialized(ref version, GetVersion);
				context.Component.Version = text;
			}
		}

		/// <summary>
		/// Loads BuildInfo.config and returns XElement.
		/// </summary>
		/// <returns></returns>
		protected virtual XElement LoadBuildInfoConfig()
		{
			XElement result = null;
			try
			{
				string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BuildInfo.config");
				if (!File.Exists(text))
				{
					CoreEventSource.Log.LogVerbose("[BuildInfoConfigComponentVersionContextInitializer] No file." + text);
					return result;
				}
				result = XDocument.Load(text).Root;
				CoreEventSource.Log.LogVerbose("[BuildInfoConfigComponentVersionContextInitializer] File loaded." + text);
				return result;
			}
			catch (XmlException ex)
			{
				CoreEventSource.Log.BuildInfoConfigBrokenXmlError(ex.Message);
				return result;
			}
		}

		/// <summary>
		/// Gets the version for the current application. If the version cannot be found, we will return the passed in default.
		/// </summary>
		/// <returns>The extracted data.</returns>
		private string GetVersion()
		{
			XElement xElement = LoadBuildInfoConfig();
			if (xElement == null)
			{
				return "Unknown";
			}
			XElement xElement2 = (from item in (from item in xElement.Descendants()
					where item.Name.LocalName == "Build"
					select item).Descendants()
				where item.Name.LocalName == "MSBuild"
				select item).Descendants().SingleOrDefault((XElement item) => item.Name.LocalName == "BuildLabel");
			if (xElement2 == null || string.IsNullOrEmpty(xElement2.Value))
			{
				return "Unknown";
			}
			return xElement2.Value;
		}
	}
}

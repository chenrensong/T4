using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>Class that contains extensions to IServiceProvider relevant to T4 templates.</summary>
	public static class ServiceProviderExtensions
	{
		/// <summary>Attempts to return a service from a remote provider enabled for COM interfacing.</summary>
		/// <returns>The associated service enabled for COM interfacing if it is possible; otherwise whatever the service provider provides. May return null.</returns>
		/// <param name="provider">The remote service provider.</param>
		/// <param name="type">The type of service being requested.</param>
		public static object GetCOMService(this IServiceProvider provider, Type type)
		{
			object service = provider.GetService(type);
			if (service == null)
			{
				return service;
			}
			try
			{
				return Marshal.GetObjectForIUnknown(Marshal.GetIUnknownForObject(service));
			}
			catch (Exception)
			{
				return service;
			}
		}
	}
}

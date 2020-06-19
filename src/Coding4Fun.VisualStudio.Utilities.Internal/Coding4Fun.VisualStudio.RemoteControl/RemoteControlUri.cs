using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.IO;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// URI with optional redirect.
	/// </summary>
	internal sealed class RemoteControlUri
	{
		private readonly Uri uri;

		/// <summary>
		/// Gets a value indicating whether the URI points to a local file.
		/// </summary>
		public bool IsLocalFile
		{
			get;
		}

		/// <summary>
		/// Gets a full URI string.
		/// </summary>
		public string FullUrl
		{
			get;
		}

		/// <summary>
		/// Gets a Host Id which was used to initialize the URI.
		/// </summary>
		public string HostId
		{
			get;
		}

		private RemoteControlUri(Uri uri, string hostId)
		{
			this.uri = uri;
			IsLocalFile = this.uri.IsFile;
			FullUrl = (this.uri.IsFile ? Path.GetFullPath(this.uri.LocalPath) : this.uri.AbsoluteUri);
			HostId = hostId;
		}

		/// <summary>
		/// Creates a new URI with optional redirect.
		/// </summary>
		/// <param name="registryTools"></param>
		/// <param name="hostId"></param>
		/// <param name="baseUrl"></param>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public static RemoteControlUri Create(IRegistryTools registryTools, string hostId, string baseUrl, string relativePath)
		{
			registryTools.RequiresArgumentNotNull("registryTools");
			hostId.RequiresArgumentNotNullAndNotEmpty("hostId");
			baseUrl.RequiresArgumentNotNullAndNotEmpty("baseUrl");
			relativePath.RequiresArgumentNotNullAndNotEmpty("relativePath");
			Uri uri = new Uri(baseUrl).AddSegment(hostId).AddSegment(relativePath);
			Tuple<Uri, string> tuple = uri.SplitLastSegment();
			string absoluteUri = tuple.Item1.AbsoluteUri;
			object registryValueFromCurrentUserRoot = registryTools.GetRegistryValueFromCurrentUserRoot("Software\\Coding4Fun\\VisualStudio\\RemoteControl\\TestUrlMapping", absoluteUri);
			if (registryValueFromCurrentUserRoot != null)
			{
				uri = new Uri(registryValueFromCurrentUserRoot.ToString()).AddSegment(tuple.Item2);
			}
			return new RemoteControlUri(uri, hostId);
		}
	}
}

using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Linq;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	internal static class UriEx
	{
		/// <summary>
		/// Split the last path segment from the given URI.
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static Tuple<Uri, string> SplitLastSegment(this Uri uri)
		{
			uri.RequiresArgumentNotNull("uri");
			UriBuilder uriBuilder = new UriBuilder(uri);
			string[] array = uriBuilder.Path.Split('/');
			int num = array.Length - 1;
			uriBuilder.Path = array.Take(num).Join("/");
			return Tuple.Create(uriBuilder.Uri, array[num]);
		}

		/// <summary>
		/// Add a path segment to the given URI. The path segment may contain trailling '/'.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="segment"></param>
		/// <returns></returns>
		public static Uri AddSegment(this Uri uri, string segment)
		{
			uri.RequiresArgumentNotNull("uri");
			segment = (segment ?? string.Empty);
			UriBuilder uriBuilder = new UriBuilder(uri);
			uriBuilder.Path = uriBuilder.Path.TrimEnd('/') + "/" + segment.TrimStart('/');
			return uriBuilder.Uri;
		}
	}
}

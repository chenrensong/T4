using System;
using System.IO;
using System.Net;

namespace Coding4Fun.VisualStudio.RemoteControl
{
	/// <summary>
	/// Holds information about the settings file, downloaded from the server or from the
	/// local IE cache. Returned by calls to methods in this class.
	/// </summary>
	internal class GetFileResult : IDisposable
	{
		/// <summary>
		/// Gets or sets a status code of the response from server
		/// </summary>
		internal HttpStatusCode Code
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a stream to read the content of the settings file.
		/// ! May be null if <seealso cref="P:Coding4Fun.VisualStudio.RemoteControl.GetFileResult.Code" /> is not OK.
		/// ! Callers must call Dispose on this object if it is not null.
		/// </summary>
		internal Stream RespStream
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this information is obtained from IE cache.
		/// </summary>
		internal bool IsFromCache
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the Age of the file in the cache if
		/// <seealso cref="P:Coding4Fun.VisualStudio.RemoteControl.GetFileResult.IsFromCache" /> is true, returns . Otherwise null.
		/// </summary>
		internal int? AgeSeconds
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the error message if <seealso cref="P:Coding4Fun.VisualStudio.RemoteControl.GetFileResult.Code" /> is not
		/// OK or NotFound.
		/// </summary>
		internal string ErrorMessage
		{
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether <seealso cref="P:Coding4Fun.VisualStudio.RemoteControl.GetFileResult.Code" /> is OK or
		/// NotFound.
		/// </summary>
		internal bool IsSuccessStatusCode
		{
			get
			{
				if (Code != HttpStatusCode.OK)
				{
					return Code == HttpStatusCode.NotFound;
				}
				return true;
			}
		}

		public void Dispose()
		{
			Stream respStream = RespStream;
			if (respStream != null)
			{
				respStream.Dispose();
				RespStream = null;
			}
		}
	}
}

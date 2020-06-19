using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Encapsulates information about a user session.
	/// </summary>
	internal sealed class SessionContextData
	{
		private readonly IDictionary<string, string> tags;

		/// <summary>
		/// Gets or sets the application-defined session ID.
		/// </summary>
		public string Id
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.SessionId);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.SessionId, value);
			}
		}

		/// <summary>
		/// Gets or sets the IsFirst Session for the user.
		/// </summary>
		public bool? IsFirst
		{
			get
			{
				return tags.GetTagBoolValueOrNull(ContextTagKeys.Keys.SessionIsFirst);
			}
			set
			{
				tags.SetTagValueOrRemove(ContextTagKeys.Keys.SessionIsFirst, value);
			}
		}

		/// <summary>
		/// Gets or sets the IsNewSession Session.
		/// </summary>
		public bool? IsNewSession
		{
			get
			{
				return tags.GetTagBoolValueOrNull(ContextTagKeys.Keys.SessionIsNew);
			}
			set
			{
				tags.SetTagValueOrRemove(ContextTagKeys.Keys.SessionIsNew, value);
			}
		}

		internal SessionContextData(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		internal void SetDefaults(SessionContextData source)
		{
			tags.InitializeTagValue(ContextTagKeys.Keys.SessionId, source.Id);
			tags.InitializeTagValue(ContextTagKeys.Keys.SessionIsFirst, source.IsFirst);
			tags.InitializeTagValue(ContextTagKeys.Keys.SessionIsNew, source.IsNewSession);
		}
	}
}

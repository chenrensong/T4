using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Encapsulates information about a user session.
	/// </summary>
	public sealed class SessionContext : IJsonSerializable
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

		internal SessionContext(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		void IJsonSerializable.Serialize(IJsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("id", Id);
			writer.WriteProperty("firstSession", IsFirst);
			writer.WriteEndObject();
		}
	}
}

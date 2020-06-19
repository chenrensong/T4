using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Encapsulates information about a user using an application.
	/// </summary>
	public sealed class UserContext : IJsonSerializable
	{
		private readonly IDictionary<string, string> tags;

		/// <summary>
		/// Gets or sets the ID of user accessing the application.
		/// </summary>
		/// <remarks>
		/// Unique user ID is automatically generated in default Application Insights configuration.
		/// </remarks>
		public string Id
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.UserId);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.UserId, value);
			}
		}

		/// <summary>
		/// Gets or sets the ID of an application-defined account associated with the user.
		/// </summary>
		public string AccountId
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.UserAccountId);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.UserAccountId, value);
			}
		}

		/// <summary>
		/// Gets or sets the UserAgent of an application-defined account associated with the user.
		/// </summary>
		public string UserAgent
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.UserAgent);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.UserAgent, value);
			}
		}

		/// <summary>
		/// Gets or sets the date when the user accessed the application for the first time.
		/// </summary>
		/// <remarks>
		/// Acquisition date is automatically supplied in default Application Insights configuration.
		/// </remarks>
		public DateTimeOffset? AcquisitionDate
		{
			get
			{
				return tags.GetTagDateTimeOffsetValueOrNull(ContextTagKeys.Keys.UserAccountAcquisitionDate);
			}
			set
			{
				tags.SetDateTimeOffsetValueOrRemove(ContextTagKeys.Keys.UserAccountAcquisitionDate, value);
			}
		}

		internal UserContext(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		void IJsonSerializable.Serialize(IJsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("id", Id);
			writer.WriteProperty("userAgent", UserAgent);
			writer.WriteProperty("accountId", AccountId);
			writer.WriteProperty("anonUserAcquisitionDate", AcquisitionDate);
			writer.WriteEndObject();
		}
	}
}

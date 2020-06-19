using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Encapsulates information about a user using an application.
	/// </summary>
	internal sealed class UserContextData
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
		/// Gets or sets the StoreRegion of an application-defined account associated with the user.
		/// </summary>
		public string StoreRegion
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.UserStoreRegion);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.UserStoreRegion, value);
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

		internal UserContextData(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		/// <summary>
		/// Sets values on the current context based on the default context passed in.
		/// </summary>
		internal void SetDefaults(UserContextData source)
		{
			tags.InitializeTagValue(ContextTagKeys.Keys.UserId, source.Id);
			tags.InitializeTagValue(ContextTagKeys.Keys.UserAgent, source.UserAgent);
			tags.InitializeTagValue(ContextTagKeys.Keys.UserAccountId, source.AccountId);
			tags.InitializeTagDateTimeOffsetValue(ContextTagKeys.Keys.UserAccountAcquisitionDate, source.AcquisitionDate);
			tags.InitializeTagValue(ContextTagKeys.Keys.UserStoreRegion, source.StoreRegion);
		}
	}
}

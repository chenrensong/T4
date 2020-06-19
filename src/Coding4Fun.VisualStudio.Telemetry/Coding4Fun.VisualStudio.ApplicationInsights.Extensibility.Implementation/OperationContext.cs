using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Encapsulates information about a user session.
	/// </summary>
	public sealed class OperationContext : IJsonSerializable
	{
		private readonly IDictionary<string, string> tags;

		/// <summary>
		/// Gets or sets the application-defined operation ID.
		/// </summary>
		public string Id
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.OperationId);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.OperationId, value);
			}
		}

		/// <summary>
		/// Gets or sets the application-defined operation NAME.
		/// </summary>
		public string Name
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.OperationName);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.OperationName, value);
			}
		}

		/// <summary>
		/// Gets or sets the application-defined operation SyntheticSource.
		/// </summary>
		public string SyntheticSource
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.OperationSyntheticSource);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.OperationSyntheticSource, value);
			}
		}

		internal OperationContext(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		void IJsonSerializable.Serialize(IJsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("id", Id);
			writer.WriteProperty("name", Name);
			writer.WriteProperty("syntheticSource", SyntheticSource);
			writer.WriteEndObject();
		}
	}
}

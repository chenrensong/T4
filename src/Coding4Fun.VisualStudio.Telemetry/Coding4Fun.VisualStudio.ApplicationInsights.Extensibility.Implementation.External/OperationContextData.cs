using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Encapsulates information about a user session.
	/// </summary>
	internal sealed class OperationContextData
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

		public string ParentId
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.OperationParentId);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.OperationParentId, value);
			}
		}

		public string RootId
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.OperationRootId);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.OperationRootId, value);
			}
		}

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

		public bool? IsSynthetic
		{
			get
			{
				return tags.GetTagBoolValueOrNull(ContextTagKeys.Keys.OperationIsSynthetic);
			}
			set
			{
				tags.SetTagValueOrRemove(ContextTagKeys.Keys.OperationIsSynthetic, value);
			}
		}

		internal OperationContextData(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		internal void SetDefaults(OperationContextData source)
		{
			tags.InitializeTagValue(ContextTagKeys.Keys.OperationId, source.Id);
			tags.InitializeTagValue(ContextTagKeys.Keys.OperationName, source.Name);
			tags.InitializeTagValue(ContextTagKeys.Keys.OperationParentId, source.ParentId);
			tags.InitializeTagValue(ContextTagKeys.Keys.OperationRootId, source.RootId);
			tags.InitializeTagValue(ContextTagKeys.Keys.OperationSyntheticSource, source.SyntheticSource);
			tags.InitializeTagValue(ContextTagKeys.Keys.OperationIsSynthetic, source.IsSynthetic);
		}
	}
}

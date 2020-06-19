namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A class that defines property names shared by multiple data model entities.
	/// PLEASE DO NOT change the constant strings defined in this class, because they're part of contract between client and backend server.
	/// </summary>
	/// <remarks>
	///  Property name is part of the contract between client API and backend data model process.
	///  Please email vsdmcrew@microsoft.com before changing the property names.
	/// </remarks>
	internal class DataModelPropertyNames
	{
		internal const string DataModelPrefix = "DataModel.";

		internal const string DataModelSource = "DataModel.Source";

		internal const string EventType = "DataModel.EntityType";

		internal const string EventSchemaVersion = "DataModel.EntitySchemaVersion";

		internal const string ProductName = "DataModel.ProductName";

		internal const string FeatureName = "DataModel.FeatureName";

		internal const string EntityName = "DataModel.EntityName";

		internal const string Severity = "DataModel.Severity";

		internal const string Correlation = "DataModel.CorrelationId";

		internal const string CorrelationPrefix = "DataModel.Correlation.";
	}
}

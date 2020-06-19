namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// All available filters, can be updated.
	/// </summary>
	public enum Filters
	{
		/// <summary>
		/// UserId which is used as primary unit for the experimentation.
		/// </summary>
		UserId,
		/// <summary>
		/// Name of the application which uses experimentation service.
		/// </summary>
		ApplicationName,
		/// <summary>
		/// Version of the application which uses experimentation service.
		/// </summary>
		ApplicationVersion,
		/// <summary>
		/// Sku of the application (VS specific - empty string if not applicable).
		/// </summary>
		ApplicationSku,
		/// <summary>
		/// branch of the application (VS specific - empty string if not applicable).
		/// </summary>
		BranchBuildFrom,
		/// <summary>
		/// Is user is Coding4Fun internal.
		/// </summary>
		IsInternal,
		/// <summary>
		/// Installation channel id.
		/// </summary>
		ChannelId
	}
}

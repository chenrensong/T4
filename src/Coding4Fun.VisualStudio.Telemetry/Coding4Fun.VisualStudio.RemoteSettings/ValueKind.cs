namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Kind of a value in the collection.
	/// </summary>
	public enum ValueKind
	{
		/// <summary>
		/// Value kind is unknown or the value doesn't exist
		/// </summary>
		Unknown,
		/// <summary>
		/// A string value
		/// </summary>
		String,
		/// <summary>
		/// Multiple strings value
		/// </summary>
		MultiString,
		/// <summary>
		/// A 32 bit value such as an int or bool.
		/// </summary>
		DWord,
		/// <summary>
		/// A 64 bit value such as a long.
		/// </summary>
		QWord
	}
}

namespace Coding4Fun.VisualStudio.Experimentation
{
	/// <summary>
	/// Storage which provides key-value pairs.
	/// </summary>
	public interface IKeyValueStorage
	{
		/// <summary>
		/// Gets current value from the storage
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		T GetValue<T>(string key, T defaultValue);

		/// <summary>
		/// Sets value to the storage.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void SetValue<T>(string key, T value);
	}
}

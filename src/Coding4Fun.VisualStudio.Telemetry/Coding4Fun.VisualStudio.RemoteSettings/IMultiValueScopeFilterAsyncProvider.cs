using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Provides a multi-value scope filter in an async way that maps a key to a specific <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.ScopeValue" />.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IMultiValueScopeFilterAsyncProvider<T> : IMultiValueScopeFilterProvider<T>, IScopeFilterProvider where T : ScopeValue
	{
		/// <summary>
		/// Provides a ScopeValue for the specified key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns>The <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.ScopeValue" /> for the passed in key.</returns>
		Task<T> ProvideAsync(string key);
	}
}

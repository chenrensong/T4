using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Provides a single <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.ScopeValue" /> in an async way.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISingleValueScopeFilterAsyncProvider<T> : ISingleValueScopeFilterProvider<T>, IScopeFilterProvider where T : ScopeValue
	{
		/// <summary>
		/// Provides a <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.ScopeValue" />
		/// </summary>
		/// <returns></returns>
		Task<T> ProvideAsync();
	}
}

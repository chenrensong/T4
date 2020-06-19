namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Provides a single <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.ScopeValue" />.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISingleValueScopeFilterProvider<out T> : IScopeFilterProvider where T : ScopeValue
	{
		/// <summary>
		/// Provides a <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.ScopeValue" />
		/// </summary>
		/// <returns></returns>
		T Provide();
	}
}

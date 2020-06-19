using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class ScopeScopeFilterProvider : IMultiValueScopeFilterAsyncProvider<BoolScopeValue>, IMultiValueScopeFilterProvider<BoolScopeValue>, IScopeFilterProvider
	{
		private readonly IScopesStorageHandler storage;

		private readonly IScopeParserFactory scopeParserFactory;

		public string Name => "Scope";

		public ScopeScopeFilterProvider(IScopesStorageHandler storage, IScopeParserFactory factory)
		{
			CodeContract.RequiresArgumentNotNull<IScopesStorageHandler>(storage, "storage");
			CodeContract.RequiresArgumentNotNull<IScopeParserFactory>(factory, "factory");
			this.storage = storage;
			scopeParserFactory = factory;
		}

		/// <summary>
		/// Handles requests for Scope.NameOfScope
		/// </summary>
		/// <param name="key">The name of the Scope to evaluate.</param>
		/// <returns>A True <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.BoolScopeValue" /> if the Scope evaluates to True.</returns>
		public BoolScopeValue Provide(string key)
		{
			string scope = storage.GetScope(key);
			if (scope != null)
			{
				return new BoolScopeValue(scopeParserFactory.Evaluate(scope));
			}
			throw new ScopeParserException("Could not find scope with name: " + key);
		}

		/// <summary>
		/// Handles async requests for Scope.NameOfScope
		/// </summary>
		/// <param name="key">The name of the Scope to evaluate.</param>
		/// <returns>A True <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.BoolScopeValue" /> if the Scope evaluates to True.</returns>
		public async Task<BoolScopeValue> ProvideAsync(string key)
		{
			string scope = storage.GetScope(key);
			if (scope != null)
			{
				return new BoolScopeValue(await scopeParserFactory.EvaluateAsync(scope).ConfigureAwait(false));
			}
			throw new ScopeParserException("Could not find scope with name: " + key);
		}
	}
}

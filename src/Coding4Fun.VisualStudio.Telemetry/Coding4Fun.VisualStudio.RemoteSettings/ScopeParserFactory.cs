using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class ScopeParserFactory : IScopeParserFactory
	{
		private readonly IDictionary<string, IScopeFilterProvider> providedFilters = new ConcurrentDictionary<string, IScopeFilterProvider>();

		private readonly IRemoteSettingsLogger logger;

		public IDictionary<string, IScopeFilterProvider> ProvidedFilters => providedFilters;

		public ScopeParserFactory(RemoteSettingsInitializer initializer)
		{
			logger = initializer.RemoteSettingsLogger;
		}

		public bool Evaluate(string scopeExpression)
		{
			try
			{
				return new ScopeParser(scopeExpression, ProvidedFilters).Run();
			}
			catch (ScopeParserException exception)
			{
				logger.LogError("Scope parsing error", exception);
				return false;
			}
		}

		public Task<bool> EvaluateAsync(string scopeExpression)
		{
			try
			{
				return new AsyncScopeParser(scopeExpression, ProvidedFilters).RunAsync();
			}
			catch (ScopeParserException exception)
			{
				logger.LogError("Scope parsing error", exception);
				return Task.FromResult(false);
			}
		}
	}
}

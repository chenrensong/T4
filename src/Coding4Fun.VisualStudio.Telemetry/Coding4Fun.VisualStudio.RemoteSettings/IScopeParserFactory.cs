using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IScopeParserFactory
	{
		IDictionary<string, IScopeFilterProvider> ProvidedFilters
		{
			get;
		}

		bool Evaluate(string scopeExpression);

		Task<bool> EvaluateAsync(string scopeExpression);
	}
}

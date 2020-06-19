using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface IScopesStorageHandler
	{
		IEnumerable<string> GetAllScopes();

		string GetScope(string scopeName);
	}
}

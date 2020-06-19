using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface ICycleDetection
	{
		bool HasCycles(IEnumerable<Scope> scopes);
	}
}

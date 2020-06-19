using System.Collections.Generic;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal interface ILocalTestParser
	{
		Task<IEnumerable<ActionResponse>> ParseStreamAsync(DirectoryReaderContext streamContext);
	}
}

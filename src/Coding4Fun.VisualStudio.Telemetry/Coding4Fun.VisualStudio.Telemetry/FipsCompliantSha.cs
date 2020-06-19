using System.Security.Cryptography;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class FipsCompliantSha
	{
		public static readonly HashAlgorithm Sha256 = HashAlgorithm.Create(typeof(SHA256CryptoServiceProvider).AssemblyQualifiedName);
	}
}

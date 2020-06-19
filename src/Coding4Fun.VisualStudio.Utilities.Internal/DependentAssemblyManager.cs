using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Coding4Fun.VisualStudio.Utilities.Internal
{
    public class DependentAssemblyManager
    {
        public static IEnumerable<string> GetRuntimeAssemblies()
        {
            // Get directory which locates System.Private.Corelib.dll
            var coreSdkAssemblyDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            // .NET Standard 2.0 library should refer netstandard.dll
            yield return Path.Combine(coreSdkAssemblyDirectory, "netstandard.dll");
            yield return typeof(object).Assembly.Location; // System.Private.Corelib.dll
            yield return typeof(Uri).Assembly.Location; // System.Private.Uri.dll
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Runtime.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Collections.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Collections.NonGeneric.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Collections.Specialized.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Diagnostics.Debug.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Diagnostics.Tools.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Globalization.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Linq.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Numerics.Vectors.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.ObjectModel.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Reflection.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Reflection.Extensions.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Reflection.Primitives.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Runtime.Extensions.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Runtime.InteropServices.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Runtime.Numerics.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Runtime.Serialization.Primitives.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Threading.dll");
            yield return Path.Combine(coreSdkAssemblyDirectory, "System.Threading.Tasks.dll");

            yield return typeof(Action<,,,,,,,,,,>).Assembly.Location; // System.Core.dll
            yield return typeof(System.Numerics.BigInteger).Assembly.Location; // System.Numerics.dll
        }
    }
}

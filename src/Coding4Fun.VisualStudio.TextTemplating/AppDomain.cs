using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Coding4Fun.Extensions.DependencyModel;

namespace Coding4Fun.VisualStudio.TextTemplating
{
    public class AppDomain
    {
        public static AppDomain CurrentDomain { get; private set; }
        public bool ShadowCopyFiles { get; internal set; }

        static AppDomain()
        {
            CurrentDomain = new AppDomain();
        }

        public Assembly[] GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            var dependencies = DependencyContext.Default.RuntimeLibraries;
            foreach (var library in dependencies)
            {
                if (IsCandidateCompilationLibrary(library))
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    assemblies.Add(assembly);
                }
            }
            return assemblies.ToArray();
        }

        private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        {
            return compilationLibrary.Name == ("Specify")
                || compilationLibrary.Dependencies.Any(d => d.Name.StartsWith("Specify"));
        }
    }
}

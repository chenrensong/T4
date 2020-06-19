using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Coding4Fun.VisualStudio.TextTemplating
{
    /// <summary>
    /// Simple run host to be used when not debugging.
    /// The use of it allows both debug and non debug runs to share the same code.
    /// </summary>
    internal class NonDebugRunFactory : IDebugTransformationRunFactory
    {
        /// <summary>
        /// The engine host
        /// </summary>
        private ITextTemplatingEngineHost host;

        /// <summary>
        /// We store the host to request an AppDomain later
        /// </summary>
        public NonDebugRunFactory(ITextTemplatingEngineHost host)
        {
            this.host = host;
        }

        /// <summary>
        /// Create an instance of TransformationRunner in the transformDomain
        /// </summary>
        public IDebugTransformationRun CreateTransformationRun(Type t, string content, Func<AssemblyLoadContext, AssemblyName, Assembly?>? resolver)
        {
            AssemblyLoadContext appDomain = null;
            try
            {
                appDomain = host.ProvideTemplatingAppDomain(content);
            }
            catch (Exception e)
            {
                if (Engine.IsCriticalException(e))
                {
                    throw;
                }
            }
            if (appDomain == null)
            {
                return null;
            }
            appDomain.Resolving += resolver;
            //appDomain.AssemblyResolve += resolver;
            var context = AssemblyLoadContext.GetLoadContext(t.Assembly);
            var assembly = context?.LoadFromAssemblyName(new AssemblyName(t.Assembly.FullName));
            return assembly?.CreateInstance(t.FullName) as IDebugTransformationRun;
            //return appDomain.CreateInstanceAndUnwrap(t.Assembly.FullName, t.FullName) as IDebugTransformationRun;
        }

        private Assembly AppDomain_Resolving(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            throw new NotImplementedException();
        }
    }
}

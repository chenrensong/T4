using Microsoft.CSharp.RuntimeBinder;
using Coding4Fun.VisualStudio.TextTemplating.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

namespace Coding4Fun.VisualStudio.TextTemplating
{
    /// <summary>
    /// Class that is instantiated in the secondary app-domain and is responsible for
    /// compiling and running the transformation code. It has to be a MarshalByRefObject
    /// </summary>
    internal sealed class TransformationRunner : MarshalByRefObject, IDebugTransformationRun
    {
        private CompilerErrorCollection errors;

        internal const string ExceptionProgressSlot = "TextTemplatingProgress";

        /// <summary>
        /// Set of paths that are shadow copied for this appdomain.
        /// </summary>
        private static HashSet<string> shadowCopyPaths = null;

        /// <summary>
        /// Marker object to allow sync on the shadowCopyPaths set. 
        /// </summary>
        private static object shadowCopySync = new object();

        /// <summary>
        /// The session we're currently in.
        /// </summary>
        private TemplateProcessingSession session;

        /// <summary>
        /// The compiled template.
        /// </summary>
        private Assembly assembly;

        /// <summary>
        /// The host.
        /// </summary>
        private ITextTemplatingEngineHost host;

        /// <summary>
        /// Cached expression built from resources from the environment assembly.
        /// </summary>
        private static Regex linePattern;

        private const string WordAtId = "Word_At";

        private const string InFileLineNumberId = "StackTrace_InFileLineNumber";

        /// <summary>
        /// A collection of the errors that are raised 
        /// </summary>
        public CompilerErrorCollection Errors
        {
            [DebuggerStepThrough]
            get
            {
                if (errors == null)
                {
                    errors = new CompilerErrorCollection();
                }
                return errors;
            }
        }

        /// <summary>
        /// This allows our object to live forever without it dying after 5 minutes of inactivity.
        /// Since it's contained in a temporary AppDomain, this shouldn't cause any issues.
        /// </summary>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Utility method for logging an error/warning
        /// </summary>
        private void LogError(string errorText, bool isWarning)
        {
            CompilerError value = new CompilerError
            {
                ErrorText = errorText,
                IsWarning = isWarning
            };
            Errors.Add(value);
        }

        /// <summary>
        /// Utility method for logging an error/warning
        /// </summary>
        private void LogError(string errorText, bool isWarning, string fileName, int line, int column)
        {
            CompilerError value = new CompilerError
            {
                FileName = fileName,
                Line = line,
                Column = column,
                ErrorText = errorText,
                IsWarning = isWarning
            };
            Errors.Add(value);
        }

        /// <summary>
        /// Since the errors collection is not marshalled by reference, we need to provide a method for
        /// the engine to call to clear them after it has reported preparation errors to the host.
        /// </summary>
        public void ClearErrors()
        {
            Errors.Clear();
        }

        /// <summary>
        /// Cache a resource from the manager or cache the fallback string if there's a problem.
        /// </summary>
        private static string GetEnvironmentResource(ResourceManager manager, string id, string fallback)
        {
            try
            {
                return manager.GetString(id) ?? fallback;
            }
            catch (MissingManifestResourceException)
            {
                return fallback;
            }
        }

        /// <summary>
        /// Utility method for parsing a line number from an exception stacktrace
        /// </summary>
        /// <remarks>
        /// This code is fragile at runtime to changes in StackTrace formatting from mscorlib, but falls back gracefully, so we can tolerate the coupling.
        /// </remarks>
        private static bool TryParseStackTrace(string stackTrace, out int lineNum, out string filename)
        {
            if (linePattern == null)
            {
                ResourceManager resourceManager = new ResourceManager("mscorlib", typeof(StackTrace).Assembly);
                resourceManager.IgnoreCase = true;
                string environmentResource = GetEnvironmentResource(resourceManager, "Word_At", "at");
                string environmentResource2 = GetEnvironmentResource(resourceManager, "StackTrace_InFileLineNumber", "in {0}:line {1}");
                resourceManager.ReleaseAllResources();
                string str = string.Format(CultureInfo.InvariantCulture, environmentResource2, "(.*)", "(\\d*)");
                string str2 = environmentResource + " (.*)";
                string pattern = str2 + str;
                linePattern = new Regex(pattern);
            }
            Match match = linePattern.Match(stackTrace);
            if (match.Success)
            {
                filename = match.Groups[2].Value;
                return int.TryParse(match.Groups[3].Value, out lineNum);
            }
            filename = string.Empty;
            lineNum = 0;
            return false;
        }

        /// <summary>
        /// Load the set of assemblies that we will need into the AppDomain
        /// </summary>
        /// <param name="assemblies"></param>
        public void PreLoadAssemblies(string[] assemblies)
        {
            try
            {
                LoadExplicitAssemblyReferences(assemblies);
            }
            catch (Exception ex)
            {
                if (Engine.IsCriticalException(ex))
                {
                    throw;
                }
                LogError(Resources.ExceptionWhileRunningCode + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
            }
        }

        /// <summary>
        /// This is the method called from the other AppDomain to compile and prepare the transformation code for running
        /// </summary>
        /// <remarks>
        /// Note that this method uses an implicit contract with the transformation object.
        /// See the DocComment on TextTransformation for details before changing the dynamic calls from this method.
        /// </remarks>
        /// <returns>True if the transformation run has been properly prepared.</returns>
        public bool PrepareTransformation(TemplateProcessingSession session, string source, ITextTemplatingEngineHost host)
        {
            this.session = session;
            this.host = host;
            ToStringHelper.FormatProvider = session.FormatProvider;
            assembly = null;
            try
            {
                this.session.AssemblyDirectives.Add(GetType().Assembly.Location);
                this.session.AssemblyDirectives.Add(typeof(ITextTemplatingEngineHost).Assembly.Location);
                assembly = LocateAssembly(session.CacheAssemblies, session.ClassFullName, source, session.TemplateFile, session.Debug, session.Language, session.AssemblyDirectives, session.CompilerOptions);
            }
            catch (Exception ex)
            {
                if (Engine.IsCriticalException(ex))
                {
                    throw;
                }
                LogError(Resources.ExceptionWhileRunningCode + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
            }
            return assembly != null;
        }

        /// <summary>
        /// This is the method called from the other AppDomain to run the transformation
        /// </summary>
        public string PerformTransformation()
        {
            string result = Resources.ErrorOutput;
            if (assembly == null)
            {
                LogError(Resources.ErrorInitializingTransformationObject, false);
                return result;
            }
            dynamic val = null;
            try
            {
                val = CreateTextTransformation(session.ClassFullName, host, assembly, session.UserTransformationSession, session.BaseClassName);
                if (val != null)
                {
                    try
                    {
                        val.Initialize();
                    }
                    catch (RuntimeBinderException arg)
                    {
                        LogError(Resources.InvalidBaseClass + string.Format(CultureInfo.CurrentCulture, Resources.Exception, arg), false);
                    }
                    catch (Exception ex)
                    {
                        if (Engine.IsCriticalException(ex))
                        {
                            throw;
                        }
                        LogError(Resources.ErrorInitializingTransformationObject + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
                    }
                    try
                    {
                        if (!val.Errors.HasErrors && !Errors.HasErrors)
                        {
                            try
                            {
                                result = val.TransformText();
                            }
                            catch (RuntimeBinderException arg2)
                            {
                                LogError(Resources.InvalidBaseClass + string.Format(CultureInfo.CurrentCulture, Resources.Exception, arg2), false);
                            }
                            catch (Exception innerException)
                            {
                                if (Engine.IsCriticalException(innerException))
                                {
                                    throw;
                                }
                                if (innerException.InnerException != null)
                                {
                                    innerException = innerException.InnerException;
                                }
                                if (innerException.Data["TextTemplatingProgress"] != null)
                                {
                                    result = innerException.Data["TextTemplatingProgress"].ToString();
                                }
                                ArgumentNullException ex2 = innerException as ArgumentNullException;
                                if (ex2 != null && StringComparer.OrdinalIgnoreCase.Compare(ex2.ParamName, "objectToConvert") == 0)
                                {
                                    int lineNum = 0;
                                    string filename = session.TemplateFile;
                                    if (session.Debug && !TryParseStackTrace(ex2.StackTrace, out lineNum, out filename))
                                    {
                                        filename = session.TemplateFile;
                                    }
                                    LogError(Resources.ExpressionBlockNull + Environment.NewLine + ex2.ToString(), false, filename, lineNum, 0);
                                }
                                else
                                {
                                    int lineNum2 = 0;
                                    string filename2 = session.TemplateFile;
                                    if (session.Debug && !TryParseStackTrace(innerException.StackTrace, out lineNum2, out filename2))
                                    {
                                        filename2 = session.TemplateFile;
                                    }
                                    LogError(Resources.TransformationErrorPrepend + innerException.ToString(), false, filename2, lineNum2, 0);
                                }
                            }
                        }
                        foreach (dynamic item in val.Errors)
                        {
                            item.ErrorText = Resources.TransformationErrorPrepend + item.ErrorText;
                            Type type = item.GetType();
                            if (type.IsEquivalentTo(typeof(CompilerError)))
                            {
                                Errors.Add(item);
                            }
                            else
                            {
                                Errors.Add(new CompilerError(item.FileName, item.Line, item.Column, item.ErrorNumber, item.ErrorText));
                            }
                        }
                        return result;
                    }
                    catch (RuntimeBinderException arg3)
                    {
                        LogError(Resources.InvalidBaseClass + string.Format(CultureInfo.CurrentCulture, Resources.Exception, arg3), false);
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex3)
            {
                if (Engine.IsCriticalException(ex3))
                {
                    throw;
                }
                LogError(Resources.ExceptionWhileRunningCode + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex3), false);
                return result;
            }
            finally
            {
                (val as IDisposable)?.Dispose();
                assembly = null;
                host = null;
                session = null;
            }
        }

        /// <summary>
        /// Find or compile an assembly with the correct code for the given source
        /// </summary>
        private Assembly LocateAssembly(bool cacheAssemblies, string fullClassName, string source, string inputFile, bool debug, SupportedLanguage language, IEnumerable<string> compilerReferences, string compilerOptions)
        {
            Assembly assembly = null;
            if (cacheAssemblies)
            {
                assembly = AssemblyCache.Find(fullClassName);
            }
            if (assembly == null)
            {
                assembly = Compile(source, inputFile, compilerReferences, debug, language, compilerOptions);
                if (assembly != null && cacheAssemblies)
                {
                    AssemblyCache.Insert(fullClassName, assembly);
                }
            }
            return assembly;
        }

        private void LoadExplicitAssemblyReferences(IEnumerable<string> references)
        {
            references = references.Where((string referenceAssembly) => !string.IsNullOrEmpty(referenceAssembly)
            && File.Exists(referenceAssembly)).ToList();
            List<string> list = new List<string>();
            if (AppDomain.CurrentDomain.ShadowCopyFiles)
            {
                foreach (string reference in references)
                {
                    string text = Path.GetDirectoryName(reference);
                    if (string.IsNullOrEmpty(text))
                    {
                        string currentDirectory = Directory.GetCurrentDirectory();
                        if (File.Exists(Path.Combine(currentDirectory, reference)))
                        {
                            text = currentDirectory;
                        }
                    }
                    list.Add(text);
                }
                EnsureShadowCopyPaths(list.Distinct(StringComparer.OrdinalIgnoreCase));
            }
            foreach (string reference2 in references)
            {
                DateTime time = default(DateTime);
                if (AppDomain.CurrentDomain.ShadowCopyFiles && File.Exists(reference2))
                {
                    FileInfo fileInfo = new FileInfo(reference2);
                    time = fileInfo.LastWriteTime;
                }
                Assembly assembly = AttemptAssemblyLoad(reference2);
                if (assembly != null && AppDomain.CurrentDomain.ShadowCopyFiles && !assembly.GlobalAssemblyCache)
                {
                    ShadowTimes.Insert(reference2, time);
                }
            }
        }

        /// <summary>
        /// Ensures that the given paths are being shadow copied.
        /// </summary>
        private static void EnsureShadowCopyPaths(IEnumerable<string> paths)
        {
            if (AppDomain.CurrentDomain.ShadowCopyFiles)
            {
                string shadowCopyPath = string.Empty;
                lock (shadowCopySync)
                {
                    if (shadowCopyPaths == null)
                    {
                        shadowCopyPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    }
                    foreach (string path in paths)
                    {
                        if (!shadowCopyPaths.Contains(path))
                        {
                            shadowCopyPaths.Add(path);
                        }
                    }
                    shadowCopyPath = string.Join(";", shadowCopyPaths.ToArray());
                }
                AppDomain.CurrentDomain.SetShadowCopyPath(shadowCopyPath);
            }
        }

        /// <summary>
        /// Instantiates an object of the generated transformation class from the compiled assembly. 
        /// </summary>
        private object CreateTextTransformation(string fullClassName, ITextTemplatingEngineHost host, Assembly assembly, ITextTemplatingSession userSession, string baseTypeName)
        {
            object obj = null;
            try
            {
                obj = assembly.CreateInstance(fullClassName);
                if (obj == null)
                {
                    LogError(Resources.ExceptionInstantiatingTransformationObject, false);
                    return null;
                }
                Type type = obj.GetType();
                if (host != null)
                {
                    try
                    {
                        PropertyInfo property = type.GetProperty("Host");
                        if (property != null)
                        {
                            property.SetValue(obj, host, null);
                        }
                        else
                        {
                            LogError(string.Format(CultureInfo.CurrentCulture, Resources.HostPropertyNotFound, baseTypeName), false);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Engine.IsCriticalException(e))
                        {
                            throw;
                        }
                        LogError(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionSettingHost, type.FullName), false);
                    }
                }
                try
                {
                    PropertyInfo mostDerivedProperty = GetMostDerivedProperty(type, "Session");
                    if (mostDerivedProperty != null)
                    {
                        mostDerivedProperty.SetValue(obj, userSession, null);
                    }
                }
                catch (Exception e2)
                {
                    if (Engine.IsCriticalException(e2))
                    {
                        throw;
                    }
                    LogError(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionSettingSession, type.FullName), false);
                }
                return obj;
            }
            catch (Exception ex)
            {
                if (Engine.IsCriticalException(ex))
                {
                    (obj as IDisposable)?.Dispose();
                    throw;
                }
                LogError(Resources.ExceptionInstantiatingTransformationObject + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
            }
            return null;
        }

        /// <summary>
        /// Get the named property as close to the most-derived class as possible.
        /// </summary>
        private static PropertyInfo GetMostDerivedProperty(Type transformationType, string propertyName)
        {
            while (transformationType != typeof(object) && transformationType != null)
            {
                PropertyInfo property = transformationType.GetProperty(propertyName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property != null)
                {
                    return property;
                }
                transformationType = transformationType.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Compiles the source into an assembly using the given CodeDomProvider and the assembly references
        /// </summary>
        private Assembly Compile(string source, string inputFile, IEnumerable<string> references, bool debug, SupportedLanguage language, string compilerOptions)
        {
            CompilerBridge compilerBridge = CompilerBridge.Create(language, source, debug, references, compilerOptions);
            Assembly result = null;
            try
            {
                CompilerBridgeOutput compilerBridgeOutput = compilerBridge.Compile();
                IEnumerable<CompilerError> diagnostics = compilerBridgeOutput.Diagnostics;
                foreach (CompilerError item in diagnostics)
                {
                    item.ErrorText = Resources.CompilerErrorPrepend + item.ErrorText;
                    if (string.IsNullOrEmpty(item.FileName))
                    {
                        item.FileName = inputFile;
                    }
                    Errors.Add(item);
                }
                if (!compilerBridgeOutput.Successful)
                {
                    return result;
                }
                result = compilerBridgeOutput.CompiledAssembly;
                return result;
            }
            catch (Exception ex)
            {
                if (Engine.IsCriticalException(ex))
                {
                    throw;
                }
                LogError(Resources.CompilerErrors + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
                return result;
            }
        }

        /// <summary>
        /// Try to load an assembly by path
        /// </summary>
        /// <returns>null if no assembly found</returns>
        private Assembly AttemptAssemblyLoad(string assemblyName)
        {
            try
            {
                if (assemblyName.Contains("System.Private.CoreLib.dll"))
                {
                    return typeof(object).Assembly;
                    //return null;
                    //https://github.com/msgpack/msgpack-cli/blob/e6b43e4f82cd529dc1dd6f2086324a32db2cbe83/test/MsgPack.UnitTest/Serialization/TempFileDependentAssemblyManager.cs
                }

                return Assembly.LoadFrom(assemblyName);
            }
            catch (Exception ex)
            {
                if (Engine.IsCriticalException(ex))
                {
                    throw;
                }
                LogError(string.Format(CultureInfo.CurrentCulture, Resources.AssemblyLoadError, assemblyName) + string.Format(CultureInfo.CurrentCulture, Resources.Exception, ex), false);
                return null;
            }
        }
    }
}

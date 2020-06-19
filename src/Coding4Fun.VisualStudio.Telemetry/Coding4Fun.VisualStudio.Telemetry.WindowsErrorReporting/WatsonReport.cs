using Coding4Fun.VisualStudio.Telemetry.SessionChannel;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting
{
	/// <summary>
	/// partial class FaultEvent, processing Exceptions and bucket information
	/// </summary>
	/// <summary>
	/// Watson Report handling
	/// </summary>
	internal class WatsonReport
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DelegateGetCLRControl(IntPtr thisPointer, out IntPtr clrControlIntPtr);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DelegateGetClrErrManager(IntPtr thisPointer, ref Guid riid, out IntPtr clrManagerErrIntPtr);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DelegateGetBucketParameters(IntPtr thisPointer, out ClrBucketParameters bucketParams);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DelegateAddRef(IntPtr thisPointer);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DelegateRelease(IntPtr thisPointer);

		private static readonly Guid clrRuntimeHostClassId = new Guid("90F1A06E-7712-4762-86B5-7A5EBA6BDB02");

		internal static object ExceptionFilterLock = new object();

		/// <summary>
		/// Default sample rate for sending FaultEvents to Watson
		/// </summary>
		public const int DefaultWatsonSamplePercent = 10;

		/// <summary>
		/// The default # of maximum Watson reports generated per telemetry session
		/// </summary>
		public const int DefaultMaximumWatsonReportsPerSession = 10;

		/// <summary>
		/// The default # of minimum seconds between Watson reports (1 hour).
		/// </summary>
		public const int DefaultMinimumSecondsBetweenWatsonReports = 3600;

		internal const string UnknownBucketValue = "_";

		internal ClrBucketParameters ClrBucketParameters;

		internal const int NumberOfBucketParameters = 10;

		internal const string ExceptionDataModelPrefix = "DataModel.Fault.Exception.";

		/// <summary>
		/// registered at the Watson back end for our event "VisualStudioNonFatalErrors2"
		/// Internally, buckets range P0-P9. Externally, P1-P10
		/// </summary>
		private static string[] bucketNames = new string[10]
		{
			"AppName",
			"AppVer",
			"TelemetryName",
			"FailureParam0",
			"FailureParam1",
			"FailureParam2",
			"FailureParam3",
			"FailureParam4",
			"NonFailureParam0",
			"NonFailureParam1"
		};

		public const int P0AppNameIndex = 0;

		public const int P1AppVersionIndex = 1;

		public const int P2TelemetryNameIndex = 2;

		public const int P3failureParam0Index = 3;

		public const int P3ExceptionTypeDefaultIndex = 3;

		public const int P4failureParam1Index = 4;

		public const int P4ModuleNameDefaultIndex = 4;

		public const int P5failureParam2Index = 5;

		public const int P5MethodNameDefaultIndex = 5;

		public const int P6failureParam3Index = 6;

		public const int P7failureParam4Index = 7;

		public const int P8nonFailureParam0Index = 8;

		public const int P9nonFailureParam1Index = 9;

		private FaultEvent FaultEvent
		{
			get;
		}

		private TelemetrySession TelemetrySession
		{
			get;
		}

		/// <summary>
		/// Gets A stringbuilder that contains any information about this error report that we want to add to WER as a file
		/// e.g. the entire nested chain of exceptions
		/// </summary>
		internal StringBuilder SBuilderErrorInfo
		{
			get;
		} = new StringBuilder();


		private void SetInitialBucketParameters(Exception exceptionObject)
		{
			Exception ex2 = null;
			if (exceptionObject != null)
			{
				AddExceptionInfoToIncludedFile(exceptionObject);
				ex2 = GetInnerMostException(exceptionObject, (Exception ex) => (!string.IsNullOrEmpty(ex.StackTrace)) ? true : false);
				GetClrWatsonExceptionInfo(ex2);
			}
			if (ClrBucketParameters.FInited == 1)
			{
				TrySetBucketParameter(0, ClrBucketParameters.Param0);
				TrySetBucketParameter(1, ClrBucketParameters.Param1);
				TrySetBucketParameter(2, FaultEvent.Name.Replace("/", "."));
				TrySetBucketParameter(3, ClrBucketParameters.ExceptionType);
				TrySetBucketParameter(4, ClrBucketParameters.Param3);
				string value = ClrBucketParameters.Param6;
				try
				{
					MethodBase targetSite = ex2.TargetSite;
					if (targetSite != null)
					{
						value = targetSite.DeclaringType?.FullName + "." + targetSite.Name;
					}
				}
				catch (Exception ex3)
				{
					SBuilderErrorInfo.AppendLine("Error while getting target site; default value used instead; this is expected for serialized exceptions:" + Environment.NewLine + FormatException(ex3));
				}
				TrySetBucketParameter(5, value);
				FaultEvent.Properties["VS.Fault.Exception.MethodDef"] = ClrBucketParameters.Param6;
				FaultEvent.Properties["VS.Fault.Exception.AppStamp"] = ClrBucketParameters.Param2;
				FaultEvent.Properties["VS.Fault.Exception.ModStamp"] = ClrBucketParameters.Param5;
				FaultEvent.Properties["VS.Fault.Exception.Offset"] = ClrBucketParameters.Param7;
				FaultEvent.Properties["VS.Fault.Exception.ModuleVersion"] = ClrBucketParameters.Param4;
			}
			else
			{
				SetBucketParametersForNoExceptionFromClr();
			}
		}

		/// <summary>
		/// if the user creates a FaultEvent and sets bucketparameters already, we won't override them
		/// </summary>
		/// <param name="bucketNum"></param>
		/// <param name="value"></param>
		private void TrySetBucketParameter(int bucketNum, string value)
		{
			if (string.IsNullOrEmpty(FaultEvent.GetBucketParameter(bucketNum)))
			{
				FaultEvent.SetBucketParameter(bucketNum, value);
			}
		}

		private void SetBucketParametersForNoExceptionFromClr()
		{
			try
			{
				Process currentProcess = Process.GetCurrentProcess();
				TrySetBucketParameter(0, Path.GetFileName(currentProcess.MainModule.FileName).ToLowerInvariant());
				string text = currentProcess.MainModule.FileVersionInfo.FileVersion;
				if (text != null)
				{
					int num = text.IndexOf(" built ", StringComparison.InvariantCultureIgnoreCase);
					if (num > 0)
					{
						text = text.Substring(0, num);
					}
				}
				TrySetBucketParameter(1, text);
				Assembly assembly = null;
				if (FaultEvent.ExceptionObject != null)
				{
					Exception innerMostException = GetInnerMostException(FaultEvent.ExceptionObject, (Exception ex) => (ex.TargetSite != null) ? true : false);
					MethodBase targetSite = innerMostException.TargetSite;
					if (targetSite != null)
					{
						assembly = targetSite.DeclaringType.Assembly;
					}
					TrySetBucketParameter(3, innerMostException.GetType().FullName);
				}
				string filename = string.Empty;
				string methodName = string.Empty;
				string offset = "0";
				int num2 = 0;
				if (assembly != null)
				{
					filename = assembly.Location;
				}
				StackTrace stackTrace = new StackTrace(false);
				StackFrame[] frames = stackTrace.GetFrames();
				foreach (StackFrame stackFrame in frames)
				{
					string text2 = stackFrame.GetMethod()?.Module?.Name?.ToLowerInvariant();
					num2 += text2.GetHashCode();
					switch (text2)
					{
					default:
						if (string.IsNullOrEmpty(text2))
						{
							continue;
						}
						break;
					case "mscorlib.dll":
					case "microsoft.visualstudio.telemetry.dll":
					case "microsoft.visualstudio.telemetry.package.dll":
					case "windowsbase.dll":
					case "system.dll":
						continue;
					}
					methodName = stackFrame.GetMethod().DeclaringType.FullName + "." + stackFrame.GetMethod().Name;
					filename = stackFrame.GetMethod().Module.Assembly.Location;
					offset = stackFrame.GetNativeOffset().ToString();
					break;
				}
				if (FaultEvent.ExceptionObject == null)
				{
					IEnumerable<StackFrame> stackFrames = RemoveFrames(stackTrace.GetFrames());
					string stackTrace2 = FormatStackTrace(stackFrames, int.MaxValue, true);
					string stack = FormatStackTrace(stackFrames);
					AddStackToTelemetryReport(stackTrace2, null, false);
					AddStackToFile("currently running Stack", stack, 0);
				}
				SetBucketParametersForModule(filename, methodName, offset);
				SBuilderErrorInfo.AppendLine($"CallStack Hash:{num2:x}");
			}
			catch (Exception ex2)
			{
				SBuilderErrorInfo.AppendLine("Exception getting module info" + FormatException(ex2));
			}
			TrySetBucketParameter(2, FaultEvent.Name.Replace("/", "."));
		}

		/// <summary>
		/// Removes telemetry frames from the top of a runtime stack trace
		/// </summary>
		/// <param name="frames">the runtime stack trace</param>
		/// <returns>the same stacktrace without the telemetry frames included</returns>
		internal static IEnumerable<StackFrame> RemoveFrames(StackFrame[] frames)
		{
			List<StackFrame> list = new List<StackFrame>();
			foreach (StackFrame stackFrame in frames)
			{
				string a = stackFrame.GetMethod().Module.ToString().ToLowerInvariant();
				if (!(a == "microsoft.visualstudio.telemetry.dll") && !(a == "microsoft.visualstudio.telemetry.package.dll"))
				{
					list.Add(stackFrame);
				}
			}
			return list;
		}

		/// <summary>
		/// A culture agnostic replacement for Exception.ToString()
		/// </summary>
		/// <param name="ex">The exception to be converted to a string</param>
		/// <returns>the string representation of the exception</returns>
		internal static string FormatException(Exception ex)
		{
			string text = FormatExceptionStack(ex);
			string text2 = string.IsNullOrEmpty(ex.Message) ? string.Empty : (ex.Message + Environment.NewLine);
			return $"{ex.GetType()}{Environment.NewLine}{text2}{text}";
		}

		/// <summary>
		/// Creates a culture agnostic string represenatation of just an exception stack trace
		/// </summary>
		/// <param name="ex">the exception</param>
		/// <param name="shorten">defaults to false, attempts to shorten the stack to mitigate length issues</param>
		/// <returns>a culture agnostic string represenatation of the exception stack trace</returns>
		internal static string FormatExceptionStack(Exception ex, bool shorten = false)
		{
			if (ex.StackTrace == null)
			{
				return string.Empty;
			}
			IEnumerable<StackFrame> frames = new StackTrace(ex, false).GetFrames();
			if (frames == null)
			{
				Regex regex = new Regex("^   \\S+ (\\S+?)\\((.*?)\\)(?: .*)?$");
				string[] array = ex.StackTrace.Split(new char[2]
				{
					'\n',
					'\r'
				}, StringSplitOptions.RemoveEmptyEntries);
				(new char[1])[0] = ',';
				(new char[1])[0] = ' ';
				StringBuilder stringBuilder = new StringBuilder();
				string[] array2 = array;
				foreach (string text in array2)
				{
					Match match = regex.Match(text);
					if (match.Success)
					{
						stringBuilder.Append(match.Groups[1].Value + "(");
						string value = match.Groups[2].Value;
						stringBuilder.Append(value);
						stringBuilder.AppendLine(")");
					}
					else if (text.Contains("End of stack trace from previous location where exception was thrown") || text.Contains("End of inner exception stack trace"))
					{
						stringBuilder.AppendLine(text);
					}
					else
					{
						stringBuilder.AppendLine(" ");
					}
				}
				return stringBuilder.ToString();
			}
			return FormatStackTrace(frames, int.MaxValue, shorten);
		}

		/// <summary>
		/// Takes any collection of StackFrame objects (from an exception or the runtime stack, typically) and converts it to a culture agnostic string representation.
		/// </summary>
		/// <param name="stackFrames">The input stack frames</param>
		/// <param name="maxLength">the maximum length. If it is longer, it will be truncated.</param>
		/// <param name="shorten">defaults to false, attempts to shorten the stack to mitigate length issues</param>
		/// <returns>a culture agnostic string representation of the stack trace</returns>
		internal static string FormatStackTrace(IEnumerable<StackFrame> stackFrames, int maxLength = int.MaxValue, bool shorten = false)
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			foreach (StackFrame stackFrame in stackFrames)
			{
				string value = FormatMethodName(stackFrame.GetMethod(), shorten);
				if (!string.IsNullOrEmpty(value))
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.Append(Environment.NewLine);
					}
					stringBuilder.Append(value);
					if (stringBuilder.Length >= maxLength)
					{
						stringBuilder.Length = maxLength;
						break;
					}
				}
			}
			return stringBuilder.ToString();
		}

		internal static string FormatMethodName(MethodBase method, bool shortened = false)
		{
			if (method == null)
			{
				return null;
			}
			StringBuilder stringBuilder = new StringBuilder(255);
			Type declaringType = method.DeclaringType;
			if (declaringType != null)
			{
				string value = declaringType.FullName.Replace('+', '.');
				stringBuilder.Append(value);
				stringBuilder.Append(".");
			}
			stringBuilder.Append(method.Name);
			MethodInfo methodInfo = method as MethodInfo;
			if (methodInfo != null && methodInfo.IsGenericMethod)
			{
				Type[] genericArguments = methodInfo.GetGenericArguments();
				stringBuilder.Append('[');
				for (int i = 0; i < genericArguments.Length; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(',');
					}
					stringBuilder.Append(genericArguments[i].Name);
				}
				stringBuilder.Append(']');
			}
			stringBuilder.Append('(');
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters != null)
			{
				for (int j = 0; j < parameters.Length; j++)
				{
					if (j != 0)
					{
						stringBuilder.Append(",");
						if (!shortened)
						{
							stringBuilder.Append(" ");
						}
					}
					string value2 = "<UnknownType>";
					if (parameters[j].ParameterType != null)
					{
						value2 = parameters[j].ParameterType.Name;
					}
					stringBuilder.Append(value2);
					if (!shortened)
					{
						stringBuilder.Append(" " + parameters[j].Name);
					}
				}
			}
			stringBuilder.Append(')');
			return stringBuilder.ToString();
		}

		private void SetBucketParametersForModule(string filename, string methodName, string offset)
		{
			string text = string.Empty;
			_ = string.Empty;
			string empty = string.Empty;
			if (!string.IsNullOrEmpty(filename))
			{
				empty = Path.GetFileNameWithoutExtension(filename);
				if (File.Exists(filename))
				{
					text = FileVersionInfo.GetVersionInfo(filename).FileVersion;
					int num = text.IndexOf(" built by", StringComparison.InvariantCultureIgnoreCase);
					if (num > 0)
					{
						text = text.Substring(0, num);
					}
				}
				TrySetBucketParameter(4, empty);
				FaultEvent.Properties["VS.Fault.Exception.ModuleVersion"] = text;
			}
			TrySetBucketParameter(5, methodName);
			FaultEvent.Properties["VS.Fault.Exception.Offset"] = offset;
		}

		/// <summary>
		/// Include a file for the exception and any inner exceptions
		/// Todo: privacy review.
		/// </summary>
		/// <param name="exceptionObject"></param>
		internal void AddExceptionInfoToIncludedFile(Exception exceptionObject)
		{
			int num = 0;
			string desc = "Exception:";
			while (exceptionObject != null)
			{
				if (exceptionObject is AggregateException)
				{
					int num2 = 0;
					foreach (Exception innerException in ((AggregateException)exceptionObject).InnerExceptions)
					{
						AddStackToFile($"Aggregate # {num2++}", FormatException(innerException), num);
					}
				}
				else
				{
					AddStackToFile(desc, FormatException(exceptionObject), num);
				}
				num++;
				desc = "Inner Exception:";
				exceptionObject = exceptionObject.InnerException;
			}
		}

		private void AddStackToFile(string desc, string stack, int indentLevel)
		{
			string str = new string(' ', indentLevel * 4);
			SBuilderErrorInfo.AppendLine();
			if (!string.IsNullOrEmpty(desc))
			{
				SBuilderErrorInfo.AppendLine(str + " " + desc);
			}
			string[] array = stack.Split(new char[2]
			{
				'\r',
				'\n'
			}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string str2 in array)
			{
				SBuilderErrorInfo.AppendLine(str + " " + str2);
			}
		}

		/// <summary>
		/// Try to rethrow the exception so CLR can calculate Watson bucket info in exception filter.
		/// see this bug about why this code is cross apartment finicky:
		/// <![CDATA[https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems#id=168457&fullScreen=false&_a=edit]]>
		/// If we can't get bucket info, calculate it manually
		/// </summary>
		/// <param name="exceptionObject"></param>
		private void GetClrWatsonExceptionInfo(Exception exceptionObject)
		{
			bool fGotIt = false;
			try
			{
				ExceptionDispatchInfo.Capture(exceptionObject).Throw();
			}
			catch (Exception ex) when (MyExceptionFilter(ex, ref fGotIt))
			{
			}
		}

		/// <summary>
		/// Exception filter to get rethrown exception bucket parameters for Watson
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="fGotIt"></param>
		/// <returns></returns>
		private unsafe bool MyExceptionFilter(Exception ex, ref bool fGotIt)
		{
			lock (ExceptionFilterLock)
			{
				IntPtr clrManagerErrIntPtr = IntPtr.Zero;
				IntPtr runtimeInterfaceAsIntPtr = RuntimeEnvironment.GetRuntimeInterfaceAsIntPtr(clrRuntimeHostClassId, typeof(IClrRuntimeHost).GUID);
				DelegateGetCLRControl obj = (DelegateGetCLRControl)Marshal.GetDelegateForFunctionPointer(*(IntPtr*)((long)(*(IntPtr*)(void*)runtimeInterfaceAsIntPtr) + (long)(IntPtr)(void*)(6L * (long)sizeof(IntPtr))), typeof(DelegateGetCLRControl));
				IntPtr clrControlIntPtr = IntPtr.Zero;
				obj(runtimeInterfaceAsIntPtr, out clrControlIntPtr);
				DelegateGetClrErrManager obj2 = (DelegateGetClrErrManager)Marshal.GetDelegateForFunctionPointer(*(IntPtr*)((long)(*(IntPtr*)(void*)clrControlIntPtr) + (long)(IntPtr)(void*)(3L * (long)sizeof(IntPtr))), typeof(DelegateGetClrErrManager));
				Guid riid = typeof(IClrErrorReportingManager).GUID;
				obj2(clrControlIntPtr, ref riid, out clrManagerErrIntPtr);
				((DelegateGetBucketParameters)Marshal.GetDelegateForFunctionPointer(*(IntPtr*)((long)(*(IntPtr*)(void*)clrManagerErrIntPtr) + (long)(IntPtr)(void*)(3L * (long)sizeof(IntPtr))), typeof(DelegateGetBucketParameters)))(clrManagerErrIntPtr, out ClrBucketParameters);
				Marshal.Release(runtimeInterfaceAsIntPtr);
			}
			return true;
		}

		private void AddPropertiesForExceptionObject(Exception exceptionObject)
		{
			if (exceptionObject == null)
			{
				return;
			}
			Exception innerMostException = GetInnerMostException(exceptionObject, (Exception ex) => (!string.IsNullOrEmpty(ex.StackTrace)) ? true : false);
			if (innerMostException != null)
			{
				exceptionObject = innerMostException;
			}
			FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.TypeString", exceptionObject.GetType().FullName);
			AddStackToTelemetryReport(FormatExceptionStack(exceptionObject, true), exceptionObject.Message);
			if (exceptionObject is TypeLoadException)
			{
				TypeLoadException ex2 = (TypeLoadException)exceptionObject;
				if (!string.IsNullOrEmpty(ex2.TypeName))
				{
					FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.TypeName", ex2.TypeName);
				}
			}
			if (exceptionObject is ObjectDisposedException)
			{
				ObjectDisposedException ex3 = (ObjectDisposedException)exceptionObject;
				if (!string.IsNullOrEmpty(ex3.ObjectName))
				{
					FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.ObjectName", ex3.ObjectName);
				}
			}
			if (exceptionObject is FileNotFoundException)
			{
				FileNotFoundException ex4 = (FileNotFoundException)exceptionObject;
				if (!string.IsNullOrEmpty(ex4.FileName))
				{
					FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.FileName", ex4.FileName);
				}
				if (!string.IsNullOrEmpty(ex4.FusionLog))
				{
					FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.FusionLog", ex4.FusionLog);
				}
			}
			if (exceptionObject is ReflectionTypeLoadException)
			{
				ReflectionTypeLoadException ex5 = (ReflectionTypeLoadException)exceptionObject;
				if (ex5.LoaderExceptions != null)
				{
					StringBuilder stringBuilder = new StringBuilder();
					Exception[] loaderExceptions = ex5.LoaderExceptions;
					foreach (Exception ex6 in loaderExceptions)
					{
						stringBuilder.AppendLine(FormatException(ex6));
					}
					FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.ExceptionArray", stringBuilder.ToString());
				}
			}
			if (exceptionObject is ArgumentException)
			{
				ArgumentException ex7 = (ArgumentException)exceptionObject;
				if (!string.IsNullOrEmpty(ex7.ParamName))
				{
					FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.ParamName", ex7.ParamName);
				}
			}
			FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.ErrorCode", exceptionObject.HResult);
		}

		private void AddStackToTelemetryReport(string stackTrace, string message = null, bool exceptionStack = true)
		{
			string userName = Environment.UserName;
			if (userName != null)
			{
				message = message?.Replace(userName, "[UserName]");
			}
			if (message != null)
			{
				FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.Message", message);
			}
			if (stackTrace != null)
			{
				TelemetryComplexProperty value = new TelemetryComplexProperty(stackTrace.Split(new char[2]
				{
					'\n',
					'\r'
				}, StringSplitOptions.RemoveEmptyEntries));
				if (exceptionStack)
				{
					FaultEvent.ReservedProperties.Add("DataModel.Fault.Exception.StackTrace", value);
				}
				else
				{
					FaultEvent.Properties.Add("VS.Fault.CallerStackTrace", value);
				}
			}
		}

		/// <summary>
		/// get inner most exception, perhaps with a constraint
		/// first get outermost to innermost in a list, then reverse the list and apply constraint
		/// </summary>
		/// <param name="exceptionObject"></param>
		/// <param name="exceptionChainRestraint"></param>
		/// <returns></returns>
		private Exception GetInnerMostException(Exception exceptionObject, Func<Exception, bool> exceptionChainRestraint)
		{
			Exception result = exceptionObject;
			List<Exception> list = new List<Exception>
			{
				exceptionObject
			};
			while (exceptionObject.InnerException != null)
			{
				list.Add(exceptionObject.InnerException);
				exceptionObject = exceptionObject.InnerException;
			}
			int count = list.Count;
			if (count != 1)
			{
				if (exceptionChainRestraint != null)
				{
					list.Reverse();
					{
						foreach (Exception item in list)
						{
							if (exceptionChainRestraint(item))
							{
								return item;
							}
						}
						return result;
					}
				}
				result = exceptionObject;
			}
			return result;
		}

		/// <summary>
		/// Given a FaultEvent, see if it's sampled. If so, call user provided callback and then process to Watson back end
		/// </summary>
		/// <param name="faultEvent"></param>
		/// <param name="telemetrySession"></param>
		/// <returns>true to post to AI back end</returns>
		public WatsonReport(FaultEvent faultEvent, TelemetrySession telemetrySession)
		{
			CodeContract.RequiresArgumentNotNull<FaultEvent>(faultEvent, "faultEvent");
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(telemetrySession, "telemetrySession");
			FaultEvent = faultEvent;
			TelemetrySession = telemetrySession;
			SBuilderErrorInfo.AppendLine("Error Information");
			SBuilderErrorInfo.AppendLine("AppInsightsEvent Name = " + FaultEvent.Name);
			SBuilderErrorInfo.AppendLine("          Description = " + FaultEvent.Description);
			SBuilderErrorInfo.AppendLine("     TelemetrySession = " + TelemetrySession.ToString());
			SBuilderErrorInfo.AppendLine("      WatsonEventType = " + FaultEvent.WatsonEventType);
			SBuilderErrorInfo.AppendLine("             UTC time = " + DateTime.UtcNow.ToString("s", DateTimeFormatInfo.InvariantInfo));
			AddPropertiesForExceptionObject(FaultEvent.ExceptionObject);
			FaultEvent.Properties["VS.Fault.WatsonEventType"] = FaultEvent.WatsonEventType;
			if (FaultEvent.WatsonEventType == "VisualStudioNonFatalErrors2" && Platform.IsWindows)
			{
				SetInitialBucketParameters(FaultEvent.ExceptionObject);
			}
			FaultEvent.ReservedProperties.Add("DataModel.Fault.Description", FaultEvent.Description);
			FaultEvent.ReservedProperties.Add("DataModel.Fault.FaultSeverity", FaultEvent.FaultSeverity.ToString());
		}

		/// <summary>
		/// Submit the report to Watson back end on Windows
		/// Execute the user provided delegate (if any) to provide more information.
		/// it's invoked from native code slightly differently: see vscommon/testtools/VSTelemetry/VSTelemetryPackage/VSFaultEvent.cs
		/// </summary>
		public void PostWatsonReport(int maxReportsPerSession, int minSecondsBetweenReports)
		{
			if (FaultEvent.IsIncludedInWatsonSample == false)
			{
				AddBucketParametersToEventProperties();
				FaultEvent.ReservedProperties.Add("DataModel.Fault.IsSampled", FaultEvent.IsIncludedInWatsonSample);
				FaultEvent.Properties["VS.Fault.WatsonOptIn"] = FaultEvent.UserOptInToWatson.ToString();
				FaultEvent.ReservedProperties["DataModel.Fault.WatsonNotSentReason"] = "NotSampled";
				LogTelemetryAboutExtraDataAddedToFaultEvent();
			}
			else
			{
				try
				{
					if (FaultEvent.GatherEventDetails != null)
					{
						try
						{
							int num = FaultEvent.GatherEventDetails(FaultEvent);
							AddBucketParametersToEventProperties();
							if (num != 0)
							{
								FaultEvent.IsIncludedInWatsonSample = false;
								FaultEvent.UserOptInToWatson = FaultEvent.FaultEventWatsonOptIn.CallbackOptOut;
								FaultEvent.ReservedProperties.Add("DataModel.Fault.IsSampled", FaultEvent.IsIncludedInWatsonSample);
								FaultEvent.Properties["VS.Fault.WatsonOptIn"] = FaultEvent.UserOptInToWatson.ToString();
								FaultEvent.ReservedProperties["DataModel.Fault.WatsonNotSentReason"] = "CallbackOptedOut";
								LogTelemetryAboutExtraDataAddedToFaultEvent();
								return;
							}
							FaultEvent.UserOptInToWatson = FaultEvent.FaultEventWatsonOptIn.CallbackOptIn;
						}
						catch (Exception ex)
						{
							FaultEvent.UserOptInToWatson = FaultEvent.FaultEventWatsonOptIn.CallbackException;
							SBuilderErrorInfo.AppendLine("Fault Event Delegate threw exception.\r\n" + ex.ToString());
							FaultEvent.ReservedProperties["DataModel.Fault.DelegateException"] = ex.ToString();
						}
					}
					else
					{
						AddBucketParametersToEventProperties();
					}
					BucketFilter matchingBucketFilter = FaultEvent.GetMatchingBucketFilter(FaultEvent.BucketFiltersToAddProcessDump, "ProcessDumpRequested");
					if (matchingBucketFilter != null)
					{
						FaultEvent.AddProcessDump(Process.GetCurrentProcess().Id);
						if (matchingBucketFilter.AdditionalProperties.TryGetValue("DumpType", out string value))
						{
							WER_DUMP_TYPE dumpTypeFromString = FaultEvent.GetDumpTypeFromString(value);
							if (dumpTypeFromString != WER_DUMP_TYPE.WerDumpTypeMax)
							{
								FaultEvent.DumpCollectionType = dumpTypeFromString;
								FaultEvent.ReservedProperties["DataModel.Fault.DumpTypeRequestedByBucketFilterId"] = matchingBucketFilter.Id;
								FaultEvent.ReservedProperties["DataModel.Fault.DumpTypeRequested"] = dumpTypeFromString.ToString();
							}
						}
					}
					LogTelemetryAboutExtraDataAddedToFaultEvent();
					FaultEvent.ReservedProperties.Add("DataModel.Fault.IsSampled", FaultEvent.IsIncludedInWatsonSample);
					FaultEvent.Properties["VS.Fault.WatsonOptIn"] = FaultEvent.UserOptInToWatson.ToString();
					bool sendParams;
					bool fullCab;
					if (!FaultEvent.AddedErrorInformation && !FaultEvent.AddedFile && !FaultEvent.AddedProcessDump && FaultEvent.UserOptInToWatson != FaultEvent.FaultEventWatsonOptIn.CallbackOptIn)
					{
						FaultEvent.ReservedProperties["DataModel.Fault.WatsonNotSentReason"] = "NoExtraDataAdded";
					}
					else if (WatsonSessionChannel.NumberOfWatsonReportsThisSession >= maxReportsPerSession)
					{
						FaultEvent.ReservedProperties["DataModel.Fault.WatsonNotSentReason"] = "MaxReportsPerSessionReached";
					}
					else
					{
						double totalSeconds = (DateTime.Now - WatsonSessionChannel.DateTimeOfLastWatsonReport).TotalSeconds;
						if (totalSeconds < (double)minSecondsBetweenReports)
						{
							FaultEvent.ReservedProperties["DataModel.Fault.WatsonNotSentReason"] = "TooSoonSinceLastReport";
							FaultEvent.ReservedProperties["DataModel.Fault.SecondsSinceLastReport"] = totalSeconds;
						}
						else if (FaultEvent.GetMatchingBucketFilter(FaultEvent.BucketFiltersToDisableWatsonReport, "WatsonReportDisabled") != null)
						{
							FaultEvent.ReservedProperties["DataModel.Fault.WatsonNotSentReason"] = "BucketFilter";
						}
						else
						{
							sendParams = (FaultEvent.UserOptInToWatson == FaultEvent.FaultEventWatsonOptIn.PropertyOptIn || FaultEvent.UserOptInToWatson == FaultEvent.FaultEventWatsonOptIn.CallbackOptIn);
							fullCab = FaultEvent.IsIncludedInWatsonSample.Value;
							if (sendParams || fullCab)
							{
								FaultEvent.Properties["VS.Fault.WatsonReportNumber"] = WatsonSessionChannel.NumberOfWatsonReportsThisSession;
								WatsonSessionChannel.NumberOfWatsonReportsThisSession++;
								WatsonSessionChannel.DateTimeOfLastWatsonReport = DateTime.Now;
								if (!sendParams || fullCab)
								{
									goto IL_04ac;
								}
								Version version = Environment.OSVersion.Version;
								if (version.Major >= 6 && (version.Major != 6 || version.Minor >= 2))
								{
									goto IL_04ac;
								}
								FaultEvent.Properties["VS.Fault.WatsonNotSentReason"] = "oldOSVersion";
							}
						}
					}
					goto end_IL_009a;
					IL_04ac:
					WER_SUBMIT_RESULT submitResult = WER_SUBMIT_RESULT.WerReportFailed;
					DateTime startTime = DateTime.MinValue;
					DateTime endTime = DateTime.MinValue;
					if (FaultEvent.SynchronousDumpCollection)
					{
						FaultEvent.ReservedProperties["DataModel.Fault.WerSubmitCurrentThread"] = true;
						try
						{
							submitResult = SendWatsonReport(fullCab, sendParams, out startTime, out endTime);
						}
						catch (Exception ex2)
						{
							LogExceptionToTelemetry(ex2);
						}
					}
					else
					{
						FaultEvent.ReservedProperties["DataModel.Fault.WerSubmitCurrentThread"] = false;
						SynchronizationContext current = SynchronizationContext.Current;
						try
						{
							SynchronizationContext.SetSynchronizationContext(NoPumpSyncContext.Default);
							ManualResetEvent mre = new ManualResetEvent(false);
							try
							{
								ThreadPool.QueueUserWorkItem(delegate
								{
									try
									{
										submitResult = SendWatsonReport(fullCab, sendParams, out startTime, out endTime);
									}
									catch (Exception ex4)
									{
										LogExceptionToTelemetry(ex4);
									}
									finally
									{
										mre.Set();
									}
								}, null);
								mre.WaitOne();
							}
							finally
							{
								if (mre != null)
								{
									((IDisposable)mre).Dispose();
								}
							}
						}
						finally
						{
							SynchronizationContext.SetSynchronizationContext(current);
						}
					}
					switch (submitResult)
					{
					case WER_SUBMIT_RESULT.WerReportQueued:
					case WER_SUBMIT_RESULT.WerReportAsync:
						if (!GetReportInfo(true, startTime, endTime))
						{
							GetReportInfo(false, startTime, endTime);
						}
						break;
					case WER_SUBMIT_RESULT.WerReportUploaded:
						GetReportInfo(false, startTime, endTime);
						break;
					}
					end_IL_009a:;
				}
				catch (Exception ex3)
				{
					LogExceptionToTelemetry(ex3);
				}
			}
		}

		private void LogTelemetryAboutExtraDataAddedToFaultEvent()
		{
			FaultEvent.ReservedProperties["DataModel.Fault.CountOfDumpsRequested"] = FaultEvent.ListProcessIdsToDump.Count;
			FaultEvent.ReservedProperties["DataModel.Fault.CountOfFilesRequested"] = FaultEvent.ListFilesToAdd.Count;
			FaultEvent.ReservedProperties["DataModel.Fault.AddedErrorInformation"] = FaultEvent.AddedErrorInformation;
		}

		private void LogExceptionToTelemetry(Exception ex)
		{
			try
			{
				TelemetryEvent telemetryEvent = new TelemetryEvent("VS/TelemetryError/FaultEvent");
				telemetryEvent.Properties["VS.TelemetryError.FaultEvent.ExceptionType"] = ex.GetType().FullName;
				TelemetrySession.PostEvent(telemetryEvent);
			}
			catch
			{
			}
		}

		private bool GetReportInfo(bool queue, DateTime startTime, DateTime endTime)
		{
			if (startTime == DateTime.MinValue)
			{
				return false;
			}
			if (!WerStoreApi.IsStoreInterfacePresent)
			{
				return false;
			}
			using (WerStoreApi.IWerStore werStore = WerStoreApi.GetStore(queue ? WerStoreApi.REPORT_STORE_TYPES.MACHINE_QUEUE : WerStoreApi.REPORT_STORE_TYPES.MACHINE_ARCHIVE))
			{
				WerStoreApi.IWerReportData werReportData = werStore.GetReports().FirstOrDefault(delegate(WerStoreApi.IWerReportData x)
				{
					if (x.EventType != FaultEvent.WatsonEventType)
					{
						return false;
					}
					for (int i = 0; i < 10; i++)
					{
						if (x.Parameters[i].Value != FaultEvent.BucketParameters[i] && (!(x.Parameters[i].Value == "_") || !string.IsNullOrEmpty(FaultEvent.BucketParameters[i])))
						{
							return false;
						}
					}
					return (!(x.TimeStamp < startTime) && !(x.TimeStamp > endTime)) ? true : false;
				});
				if (werReportData != null)
				{
					FaultEvent.Properties["VS.Fault.ReportID"] = werReportData.ReportId.ToString();
					if (!string.IsNullOrEmpty(werReportData.CabID))
					{
						FaultEvent.Properties["VS.Fault.CabID"] = werReportData.CabID;
					}
					if (werReportData.BucketId != Guid.Empty)
					{
						FaultEvent.Properties["VS.Fault.BucketID"] = werReportData.BucketId.ToString();
					}
					return true;
				}
			}
			return false;
		}

		private WER_SUBMIT_RESULT SendWatsonReport(bool fullCab, bool sendParams, out DateTime startTime, out DateTime endTime)
		{
			IntPtr intPtr = WerReportShim.WerReportCreate(FaultEvent.WatsonEventType, WER_REPORT_TYPE.WerReportNonCritical, IntPtr.Zero);
			if (fullCab)
			{
				WER_DUMP_TYPE result = WER_DUMP_TYPE.WerDumpTypeMiniDump;
				if (FaultEvent.Properties.TryGetValue("DumpCollectionType", out object value) && !string.IsNullOrEmpty(value as string))
				{
					if (Enum.TryParse(value as string, true, out result))
					{
						FaultEvent.DumpCollectionType = result;
					}
					else
					{
						FaultEvent.Properties["ErrorInvalidDumpCollectionType"] = (value as string);
					}
				}
				FaultEvent.ReservedProperties["DataModel.Fault.DumpCollectionType"] = FaultEvent.DumpCollectionType.ToString();
				int num = 0;
				foreach (int item in FaultEvent.ListProcessIdsToDump)
				{
					try
					{
						Process processById = Process.GetProcessById(item);
						if (processById != null)
						{
							SBuilderErrorInfo.AppendLine($"WerReportAddDump PID={item} {processById.ProcessName} {FaultEvent.DumpCollectionType}");
							WerReportShim.WerReportAddDump(intPtr, processById.Handle, IntPtr.Zero, FaultEvent.DumpCollectionType, IntPtr.Zero, IntPtr.Zero, 0);
							num++;
						}
					}
					catch (Exception ex)
					{
						SBuilderErrorInfo.AppendLine("Fault Event Dump Process threw exception.\r\n" + ex.ToString());
					}
				}
				FaultEvent.ReservedProperties["DataModel.Fault.CountOfDumpsAdded"] = num;
				try
				{
					int dwFileFlags = 2;
					if (FaultEvent.SBuilderAdditionalUserErrorInfo.Length > 0)
					{
						SBuilderErrorInfo.AppendLine("Additional Error info marked as not-anonymous (potentially contains PII)");
						SBuilderErrorInfo.Append(FaultEvent.SBuilderAdditionalUserErrorInfo);
						dwFileFlags = 0;
					}
					if (SBuilderErrorInfo.Length != 0)
					{
						string text = Path.Combine(Path.GetTempPath(), "VSFaultInfo" + Path.DirectorySeparatorChar.ToString() + DateTime.Now.ToString("yyMMdd_hhmmss_fffffff"));
						if (!Directory.Exists(text))
						{
							Directory.CreateDirectory(text);
						}
						string text2 = Path.Combine(text, "ErrorInformation.txt");
						if (File.Exists(text2))
						{
							File.Delete(text2);
						}
						string text3 = SBuilderErrorInfo.ToString();
						FaultEvent.ReservedProperties["DataModel.Fault.LengthOfErrorInformation"] = text3.Length;
						File.WriteAllText(text2, text3);
						WerReportShim.WerReportAddFile(intPtr, text2, WER_FILE_TYPE.WerFileTypeOther, dwFileFlags);
					}
					int num2 = 0;
					long num3 = 0L;
					foreach (string item2 in FaultEvent.ListFilesToAdd)
					{
						if (File.Exists(item2))
						{
							num3 += new FileInfo(item2).Length;
							WerReportShim.WerReportAddFile(intPtr, item2, WER_FILE_TYPE.WerFileTypeOther, 0);
							num2++;
						}
					}
					FaultEvent.ReservedProperties["DataModel.Fault.CountOfFilesAdded"] = num2;
					FaultEvent.ReservedProperties["DataModel.Fault.SizeOfFilesAdded"] = num3;
				}
				catch
				{
				}
			}
			bool flag = FaultEvent.WatsonEventType == "VisualStudioNonFatalErrors2";
			for (int i = 0; i < 10; i++)
			{
				string text4 = FaultEvent.BucketParameters[i];
				if (flag)
				{
					if (string.IsNullOrEmpty(text4))
					{
						text4 = "_";
					}
					WerReportShim.WerReportSetParameter(intPtr, i, bucketNames[i], text4);
				}
				else if (!string.IsNullOrEmpty(text4))
				{
					WerReportShim.WerReportSetParameter(intPtr, i, null, text4);
				}
			}
			int dwFlags;
			if (!fullCab && sendParams)
			{
				dwFlags = 4128;
			}
			else
			{
				if (!fullCab)
				{
					WerReportShim.WerReportCloseHandle(intPtr);
					startTime = DateTime.MinValue;
					endTime = DateTime.MinValue;
					return WER_SUBMIT_RESULT.WerReportCancelled;
				}
				dwFlags = 32;
			}
			startTime = DateTime.UtcNow;
			WER_SUBMIT_RESULT result2 = (WER_SUBMIT_RESULT)WerReportShim.WerReportSubmit(intPtr, WER_CONSENT.WerConsentNotAsked, dwFlags);
			endTime = DateTime.UtcNow;
			TimeSpan timeSpan = endTime - startTime;
			FaultEvent.ReservedProperties["DataModel.Fault.WerSubmitDurationInMs"] = timeSpan.TotalMilliseconds;
			FaultEvent.ReservedProperties["DataModel.Fault.WerSubmitResult"] = result2.ToString();
			WerReportShim.WerReportCloseHandle(intPtr);
			return result2;
		}

		private void AddBucketParametersToEventProperties()
		{
			for (int i = 0; i < 10; i++)
			{
				FaultEvent.ReservedProperties[$"DataModel.Fault.BucketParam{i + 1}"] = FaultEvent.BucketParameters[i];
			}
		}
	}
}

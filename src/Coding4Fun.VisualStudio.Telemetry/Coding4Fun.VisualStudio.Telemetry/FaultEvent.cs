using Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// A telemetry event representing a Fault, such as an exception
	/// We have 2 back ends to send data: the Telemetry back end and the Watson (back end).
	/// Cross platform, (as on Mac, Linux), we can use the same architecture.
	/// For example, on Mac, there's Merp, the Max implementation of Windows Error Reporting.
	/// FaultEvent inherits from TelemetryEvent
	/// User can create an instance of this class directly and can add custom properties directly on the class without using call back.
	/// After creating one of these, call Session.PostFault(faultEvent) which will call the callback, post the event to Watson (if sampled) and Post as a normal telemetry event
	/// Or you can use TelemetrySession.PostFault() rather than this class directly.
	/// </summary>
	public sealed class FaultEvent : TelemetryEvent, IFaultUtility
	{
		internal enum FaultEventWatsonOptIn
		{
			Unspecified,
			PropertyOptIn,
			CallbackOptIn,
			PropertyOptOut,
			CallbackOptOut,
			CallbackException
		}

		/// <summary>
		/// used for real faults (not tests) that occur within the telemetry assembly itself
		/// </summary>
		internal const string InternalFaultEventName = "VS/Telemetry/InternalFault";

		internal const int WatsonMaxParamLength = 255;

		/// <summary>
		/// Gets or sets the filters used to disable Watson reports for fault events matching a given set of bucket parameters.
		/// </summary>
		public static List<BucketFilter> BucketFiltersToDisableWatsonReport = new List<BucketFilter>();

		/// <summary>
		/// Gets or sets the filters used to add process dumps to fault events matching a given set of bucket parameters.
		/// </summary>
		public static List<BucketFilter> BucketFiltersToAddProcessDump = new List<BucketFilter>();

		internal const string watsonEventTypeVisualStudioNonFatalErrors2 = "VisualStudioNonFatalErrors2";

		/// <summary>
		/// This keeps track of whether and how the consumer of this API opted into sending data to Watson.
		/// </summary>
		internal FaultEventWatsonOptIn UserOptInToWatson;

		/// <summary>
		/// Gets or sets the sample rate used to calculate whether or not a qualifying fault event will be reported to Watson.
		/// A fault event qualifies for reporting to Watson if it is modified in at least one of the following methods is called on it:
		/// 1) AddErrorInformation
		/// 2) AddFile
		/// 3) AddProcessDump
		/// </summary>
		public static int? WatsonSamplePercent
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the maximum number of fault events that will be reported to Watson during the telemetry session.
		/// </summary>
		public static int? MaximumWatsonReportsPerSession
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the minimum number of seconds that must elapse after a Watson report is sent for a fault event before another report can be sent.
		/// </summary>
		public static int? MinimumSecondsBetweenWatsonReports
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether we sample this event locally. Affects Watson only.
		/// If false, will not send to Watson: only sends the telemetry event to AI and doesn't call callback.
		/// Changing this will force the event to send to Watson. Be careful because it can have big perf impact.
		/// If unchanged, it will be set according to the default sample rate. See <see cref="F:Coding4Fun.VisualStudio.Telemetry.WindowsErrorReporting.WatsonReport.DefaultWatsonSamplePercent" />
		/// </summary>
		public bool? IsIncludedInWatsonSample
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the type of dump that's created for AddProcessDump and sent to Watson
		/// AddProcessDump indicates which processes to dump, and DumpCollectionType determines the kind of dump
		/// To get a full heap dump, set this value to WER_DUMP_TYPE.WerDumpTypeHeapDump.
		/// e.g. (ev as FaultEvent).DumpCollectionType = WER_DUMP_TYPE.WerDumpTypeHeapDump;
		/// Another way to set this property is to add a normal Telemetry Property into the TelemetryEvent Property Bag
		/// e.g.             faultEvent.Properties["DUMPCOLLECTIONTYPE"] = "werdumptypeheapdump"; //works from native code, uses Enum.Parse case insensitive.
		///   The property bag setting (usable from native code) will override the Property setting (which is much more discoverable in intellisense)
		/// When calling TelemetrySession.PostEvent(faultEvent), WerReportAddDump is called for each process in AddProcesDump with the DumpCollectionType specified.
		/// All processes dumped will have the same DumpCollectionType
		/// You can control the type of dump (and even whether to send a dump) via remote settings in your GatherEventDetails callback
		/// Very useful for collecting heap dumps for those rare cases that are very difficult to debug.
		/// Once the issue is fixed, Remote settings can turn this off.
		/// Defaults to WER_DUMP_TYPE.WerDumpTypeMiniDump
		/// </summary>
		public WER_DUMP_TYPE DumpCollectionType
		{
			get;
			set;
		} = WER_DUMP_TYPE.WerDumpTypeMiniDump;


		/// <summary>
		/// Gets or sets This must be an event type registered on the Watson back end like "VisualStudioNonFatalErrors2".
		/// All "normal" FaultEvents should go to VisualStudioNonFatalErrors2
		/// Various Watson event types behave differently. For example the # and retention policy of collected cabs, the routing of cabs.
		/// e.g. "VisualStudioMemWatson" is used to collect a stream of cabs to be processed by the PerfWatson backend.
		/// These events can be queried from http://Watson .
		/// </summary>
		public string WatsonEventType
		{
			get;
			set;
		} = "VisualStudioNonFatalErrors2";


		/// <summary>
		/// Gets or sets a value indicating whether we capture the dump file synchronously or on the threadpool.
		/// If we're collecting a dump due to ThreadPool starvation, we don't want to use the ThreadPool to collect the dump
		/// (the Threadpool is starved and by the time the dump code is run, the pool is drained)
		/// </summary>
		public bool SynchronousDumpCollection
		{
			get;
			set;
		}

		internal string Description
		{
			get;
		}

		internal Exception ExceptionObject
		{
			get;
		}

		internal Func<IFaultUtility, int> GatherEventDetails
		{
			get;
		}

		internal FaultSeverity FaultSeverity
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this faultevent should be posted to AI. (telemetry internal failures can cause infinite recursion)
		/// </summary>
		internal bool PostThisEventToTelemetry
		{
			get;
			set;
		} = true;


		internal string[] BucketParameters
		{
			get;
			set;
		} = new string[10];


		internal List<int> ListProcessIdsToDump
		{
			get;
		} = new List<int>();


		internal List<string> ListFilesToAdd
		{
			get;
		} = new List<string>();


		/// <summary>
		/// Gets A stringbuilder that contains any information about this error report that we want to add to WER as a file
		/// callback users can add strings
		/// </summary>
		internal StringBuilder SBuilderAdditionalUserErrorInfo
		{
			get;
		} = new StringBuilder();


		/// <summary>
		/// Gets or sets a value indicating whether or not the FaultEvent owner has modified the event in a way that would make it eligible for reporting to Watson.
		/// We don't want to report default FaultEvents to Watson because it can have performance impact, and all default properties are posted by VS Telemetry.
		/// </summary>
		internal bool AddedErrorInformation
		{
			get;
			set;
		}

		internal bool AddedProcessDump
		{
			get;
			set;
		}

		internal bool AddedFile
		{
			get;
			set;
		}

		internal BucketFilter GetMatchingBucketFilter(List<BucketFilter> bucketFilters, string bucketFilterTelemetryPropertyNamePrefix)
		{
			foreach (BucketFilter bucketFilter in bucketFilters)
			{
				if (bucketFilter.WatsonEventType == WatsonEventType)
				{
					bool flag = false;
					for (int i = 0; i < 10; i++)
					{
						string text = bucketFilter.BucketParameterFilters[i];
						if (text != null)
						{
							string text2 = BucketParameters[i];
							if (text2 == null)
							{
								flag = true;
								break;
							}
							if (!new Regex(text).Match(text2).Success)
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						base.ReservedProperties["DataModel.Fault." + bucketFilterTelemetryPropertyNamePrefix + "ByBucketFilterId"] = bucketFilter.Id;
						return bucketFilter;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Create an uncategorized severity FaultEvent.
		/// The pattern:
		/// 1. FEvent = new FaultEvent(...)
		/// 2. tsession.PostEvent(FEvent)        //posts the event to Watson and AI
		/// External users should call the TelemetrySession extension methods "PostFault" (which calls PostEvent)
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description"></param>
		/// <param name="exceptionObject"></param>
		/// <param name="gatherEventDetails">This delegate is called to gather expensive details (like jscript call stacks) only when not sampled.
		/// The callback parameter can be cast to to a FaultEvent or (IVsFaultEvent in native) which inherits from TelemetryEvent (IVsTelemetryEvent in native)
		/// <seealso cref="T:Coding4Fun.VisualStudio.Telemetry.IFaultUtility" />
		/// </param>
		public FaultEvent(string eventName, string description, Exception exceptionObject = null, Func<IFaultUtility, int> gatherEventDetails = null)
			: this(eventName, description, FaultSeverity.Uncategorized, exceptionObject, gatherEventDetails)
		{
		}

		/// <summary>
		/// Create a FaultEvent.
		/// The pattern:
		/// 1. FEvent = new FaultEvent(...)
		/// 2. tsession.PostEvent(FEvent)        //posts the event to Watson and AI
		/// External users should call the TelemetrySession extension methods "PostFault" (which calls PostEvent)
		/// It becomes more useful when correlated with <see cref="T:Coding4Fun.VisualStudio.Telemetry.UserTaskEvent" /> or <see cref="T:Coding4Fun.VisualStudio.Telemetry.OperationEvent" /> which may have led to the fault occurence.
		/// </summary>
		/// <param name="eventName">
		/// An event name following data model schema.
		/// It requires that event name is a unique, not null or empty string.
		/// It consists of 3 parts and must follows pattern [product]/[featureName]/[entityName]. FeatureName could be a one-level feature or feature hierarchy delimited by "/".
		/// For examples,
		/// vs/platform/opensolution;
		/// vs/platform/editor/lightbulb/fixerror;
		/// </param>
		/// <param name="description"></param>
		/// <param name="faultSeverity">The severity of the fault, used to identify actionable or important faults in divisional tools and reporting.</param>
		/// <param name="exceptionObject"></param>
		/// <param name="gatherEventDetails">This delegate is called to gather expensive details (like jscript call stacks) only when not sampled.
		/// The callback parameter can be cast to to a FaultEvent or (IVsFaultEvent in native) which inherits from TelemetryEvent (IVsTelemetryEvent in native)
		/// <seealso cref="T:Coding4Fun.VisualStudio.Telemetry.IFaultUtility" />
		/// </param>
		public FaultEvent(string eventName, string description, FaultSeverity faultSeverity, Exception exceptionObject = null, Func<IFaultUtility, int> gatherEventDetails = null)
			: base(eventName, TelemetrySeverity.High, DataModelEventType.Fault)
		{
			Description = (description ?? string.Empty);
			ExceptionObject = exceptionObject;
			GatherEventDetails = gatherEventDetails;
			FaultSeverity = faultSeverity;
			UserOptInToWatson = FaultEventWatsonOptIn.Unspecified;
			DataModelEventNameHelper.SetProductFeatureEntityName(this);
		}

		/// <summary>
		/// Each Watson cab includes a text file (ErrrorInformation.txt) with basic information such as Error.ToString,
		/// telemetry properties, etc.
		/// Call this to add information to the file
		/// This does the work of creating a unique temp file name per instance and adding standard information
		/// NOTE: Calling this will result in the ErrorInformation.txt being marked for PII, and therefore block
		/// the CAB upload unless the users' telemetry settings are Full.
		/// </summary>
		/// <param name="information"></param>
		public void AddErrorInformation(string information)
		{
			if (!string.IsNullOrEmpty(information))
			{
				AddedErrorInformation = true;
				string[] array = information.Split(new char[2]
				{
					'\r',
					'\n'
				}, StringSplitOptions.RemoveEmptyEntries);
				SBuilderAdditionalUserErrorInfo.AppendLine();
				Array.ForEach(array, delegate(string line)
				{
					SBuilderAdditionalUserErrorInfo.AppendLine("   " + line);
				});
			}
		}

		/// <summary>
		/// NOTE: When using FaultEvent from VisualStudio, you are strongly encouraged to not directly call AddProcessDump for the current process,
		/// but instead remotely trigger it with a targeted notification. Search for "APPLYING BUCKET FILTERS TO FAULT EVENTS" in the VS repo for details.
		///
		/// Add the process id of a process for which to collect a dump
		/// Dump collection doese not occur unless the Watson back end requests a dump
		/// You can request a heap dump for a particular bucket from the Watson portal: https://watsonportal.microsoft.com/.
		/// Dump collection is out of process, to reduce chance of deadlock
		/// </summary>
		/// <param name="pid"></param>
		public void AddProcessDump(int pid)
		{
			AddedProcessDump = true;
			ListProcessIdsToDump.Add(pid);
		}

		/// <summary>
		/// Set the bucket parameter for a Watson issue
		/// Should not contain any full file paths or PII
		/// A unique set of 10 bucket parameters constitues a BucketId, which is considered the same failure.
		/// When passing in an Exception Object, the bucket parameters are set by the IClrErrorReportingManager::GetBucketParametersForCurrentException
		/// </summary>
		/// <param name="bucketNumber">int 0-9</param>
		/// <param name="newBucketValue">max len 255. Can be queried at http://Watson </param>
		public void SetBucketParameter(int bucketNumber, string newBucketValue)
		{
			if (bucketNumber >= 0 && bucketNumber < 10)
			{
				if (string.IsNullOrEmpty(newBucketValue))
				{
					newBucketValue = string.Empty;
				}
				BucketParameters[bucketNumber] = TrucateToMaxWatsonParamLength(newBucketValue.Trim());
			}
		}

		/// <summary>
		/// Set the bucket parameter attributed to the fault's reported app name as processed in Watson and elsewhere. This is automatically set in most instances.
		/// You should only change the reported app name in very special circumstances where the app reporting telemetry isn't also the app experiencing the fault.
		/// </summary>
		/// <param name="appName">The name of the app</param>
		public void SetAppName(string appName)
		{
			SetBucketParameter(0, appName);
		}

		/// <summary>
		/// Set the bucket parameter attributed to the fault's reported app version as processed in Watson and elsewhere. This is automatically set in most instances.
		/// You should only change the reported app version in very special circumstances where the app reporting telemetry isn't also the app experiencing the fault.
		/// </summary>
		/// <param name="appVersion">The version of the application</param>
		public void SetAppVersion(string appVersion)
		{
			SetBucketParameter(1, appVersion);
		}

		/// <summary>
		/// Set the bucket parameters which compose the unique Failure ID for the reported fault as processed in Watson and elsewhere. Failure parameters which are left unspecified or passed a null value will retain their original value set by default.
		/// See the DevDiv wiki for additional documentation. This is the recommended API to use for customizing the unique failure identification process for your team's faults.
		/// </summary>
		/// <param name="failureParameter0">The parameter value for Failure Parameter 0</param>
		/// <param name="failureParameter1">The parameter value for Failure Parameter 1</param>
		/// <param name="failureParameter2">The parameter value for Failure Parameter 2</param>
		/// <param name="failureParameter3">The parameter value for Failure Parameter 3</param>
		/// <param name="failureParameter4">The parameter value for Failure Parameter 4</param>
		public void SetFailureParameters(string failureParameter0 = null, string failureParameter1 = null, string failureParameter2 = null, string failureParameter3 = null, string failureParameter4 = null)
		{
			List<string> list = new List<string>
			{
				failureParameter0,
				failureParameter1,
				failureParameter2,
				failureParameter3,
				failureParameter4
			};
			for (int i = 0; i < list.Count; i++)
			{
				int bucketNumber = i + 3;
				string newBucketValue = list[i] ?? GetBucketParameter(bucketNumber);
				SetBucketParameter(bucketNumber, newBucketValue);
			}
		}

		/// <summary>
		/// Set the bucket parameters which compose the non-failure parameters as processed in Watson and elsewhere. Parameters which are left unspecified or passed a null value will retain their original value set by default.
		/// </summary>
		/// <param name="nonFailureParameter0">The parameter value for Non-Failure Parameter 0</param>
		/// <param name="nonFailureParameter1">The parameter value for Non-Failure Parameter 1</param>
		public void SetNonFailureParameters(string nonFailureParameter0 = null, string nonFailureParameter1 = null)
		{
			SetBucketParameter(8, nonFailureParameter0 ?? GetBucketParameter(8));
			SetBucketParameter(9, nonFailureParameter1 ?? GetBucketParameter(9));
		}

		internal static string TrucateToMaxWatsonParamLength(string input)
		{
			if (input.Length > 255)
			{
				return input.Substring(0, 255);
			}
			return input;
		}

		/// <summary>
		/// Get the value of a bucket parameter
		/// </summary>
		/// <param name="bucketNumber">int 0-9</param>
		/// <returns></returns>
		public string GetBucketParameter(int bucketNumber)
		{
			string result = string.Empty;
			if (bucketNumber >= 0 && bucketNumber < 10)
			{
				result = BucketParameters[bucketNumber];
			}
			return result;
		}

		/// <summary>
		/// add a file to the report sent back to Coding4Fun
		/// </summary>
		/// <param name="fullPathFileName"></param>
		public void AddFile(string fullPathFileName)
		{
			AddedFile = true;
			ListFilesToAdd.Add(fullPathFileName);
		}

		/// <summary>
		/// ToString to make debugging easier: show in debug watch window
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"{base.ToString()} IsSampled = {IsIncludedInWatsonSample}";
		}

		internal static WER_DUMP_TYPE GetDumpTypeFromString(string dumpType)
		{
			dumpType = dumpType.ToUpper();
			if (dumpType == "MICRO")
			{
				return WER_DUMP_TYPE.WerDumpTypeMicroDump;
			}
			if (dumpType == "MINI")
			{
				return WER_DUMP_TYPE.WerDumpTypeMiniDump;
			}
			if (dumpType == "HEAP")
			{
				return WER_DUMP_TYPE.WerDumpTypeHeapDump;
			}
			if (dumpType == "TRIAGE")
			{
				return WER_DUMP_TYPE.WerDumpTypeTriageDump;
			}
			return WER_DUMP_TYPE.WerDumpTypeMax;
		}
	}
}

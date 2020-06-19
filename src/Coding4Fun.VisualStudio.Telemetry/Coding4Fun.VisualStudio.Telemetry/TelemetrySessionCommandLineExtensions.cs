using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Extension methods for <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetrySession" /> to post command line arguments.
	/// </summary>
	public static class TelemetrySessionCommandLineExtensions
	{
		private struct CommandLineFlagsInfo
		{
			public int ArgsCount;

			public string[] Flags;
		}

		internal const string CommandLineFlagsEventName = "vs/telemetryapi/commandlineflags";

		internal const string CommandLineFlagsPropertiesPrefix = "VS.TelemetryApi.CommandLineFlags.";

		internal const string CommandLineFlagsArgumentsCountProperty = "VS.TelemetryApi.CommandLineFlags.ArgsCount";

		internal const string CommandLineFlagsFlagsCountProperty = "VS.TelemetryApi.CommandLineFlags.FlagsCount";

		internal const string CommandLineFlagsFirstFlagProperty = "VS.TelemetryApi.CommandLineFlags.FirstFlag";

		internal const string CommandLineFlagsFlagsProperty = "VS.TelemetryApi.CommandLineFlags.Flags";

		/// <summary>
		/// Gets or sets the function to provide command-line arguments for the current process.
		/// </summary>
		/// <remarks>Internal property to allow dependency injection from unit tests.</remarks>
		internal static Func<string[]> GetCommandLineArgsFunc
		{
			get;
			set;
		} = Environment.GetCommandLineArgs;


		/// <summary>
		/// Queues a telemetry event with command line flags information to be posted to the server.
		/// Only command line flags (identified by the given prefixes) will be included.
		/// </summary>
		/// <param name="session">A <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetrySession" /> to post the event with.</param>
		/// <param name="flagPrefixes">The prefix(s) to identify a program's flag.</param>
		/// <exception cref="T:System.ArgumentNullException">If session is null.</exception>
		/// <exception cref="T:System.ArgumentException">If eventName is null, empty or white space.</exception>
		/// <exception cref="T:System.ArgumentException">
		/// If no prefixes are specified, or all prefixes are null, empty or white space.
		/// </exception>
		public static void PostCommandLineFlags(this TelemetrySession session, params string[] flagPrefixes)
		{
			session.PostCommandLineFlags(flagPrefixes, null);
		}

		/// <summary>
		/// Queues a telemetry event with command line flags information with additional properties to be posted to the server.
		/// Only command line flags (identified by the given prefixes) will be included.
		/// </summary>
		/// <param name="session">A <see cref="T:Coding4Fun.VisualStudio.Telemetry.TelemetrySession" /> to post the event with.</param>
		/// <param name="flagPrefixes">The prefix(s) to identify a program's flag.</param>
		/// <param name="additionalProperties">Optional additional properties to include with the event.</param>
		/// <exception cref="T:System.ArgumentNullException">If session is null.</exception>
		/// <exception cref="T:System.ArgumentException">If eventName is null, empty or white space.</exception>
		/// <exception cref="T:System.ArgumentException">
		/// If no prefixes are specified, or all prefixes are null, empty or white space.
		/// </exception>
		public static void PostCommandLineFlags(this TelemetrySession session, IEnumerable<string> flagPrefixes, IDictionary<string, object> additionalProperties)
		{
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(session, "session");
			CommandLineFlagsInfo commandLineFlagsInfo = ComputeCommandLineFlags(flagPrefixes);
			TelemetryEvent telemetryEvent = new TelemetryEvent("vs/telemetryapi/commandlineflags");
			telemetryEvent.Properties["VS.TelemetryApi.CommandLineFlags.ArgsCount"] = commandLineFlagsInfo.ArgsCount;
			telemetryEvent.Properties["VS.TelemetryApi.CommandLineFlags.FlagsCount"] = commandLineFlagsInfo.Flags.Length;
			telemetryEvent.Properties["VS.TelemetryApi.CommandLineFlags.FirstFlag"] = ((commandLineFlagsInfo.Flags.Length != 0) ? commandLineFlagsInfo.Flags[0] : string.Empty);
			telemetryEvent.Properties["VS.TelemetryApi.CommandLineFlags.Flags"] = new TelemetryComplexProperty(commandLineFlagsInfo.Flags);
			if (additionalProperties != null)
			{
				foreach (KeyValuePair<string, object> additionalProperty in additionalProperties)
				{
					telemetryEvent.Properties[additionalProperty.Key] = additionalProperty.Value;
				}
			}
			session.PostEvent(telemetryEvent);
		}

		private static CommandLineFlagsInfo ComputeCommandLineFlags(IEnumerable<string> flagPrefixes)
		{
			CodeContract.RequiresArgumentNotNull<IEnumerable<string>>(flagPrefixes, "flagPrefixes");
			string[] prefixes = (from fp in flagPrefixes
				where !string.IsNullOrWhiteSpace(fp)
				select fp.Trim()).ToArray();
			if (prefixes.Length == 0)
			{
				throw new ArgumentException("All flag prefixes are invalid (null, empty or white space).", "flagPrefixes");
			}
			string[] array = GetCommandLineArgs().Skip(1).ToArray();
			string[] flags = (from arg in array
				select arg.TryGetValidFlag(prefixes) into r
				where r.Item1
				select r.Item2).ToArray();
			CommandLineFlagsInfo result = default(CommandLineFlagsInfo);
			result.ArgsCount = array.Length;
			result.Flags = flags;
			return result;
		}

		private static string[] GetCommandLineArgs()
		{
			string[] array = null;
			try
			{
				array = GetCommandLineArgsFunc();
			}
			catch
			{
			}
			if (array == null)
			{
				array = new string[0];
			}
			return array;
		}

		private static Tuple<bool, string> TryGetValidFlag(this string argument, string[] flagPrefixes)
		{
			Tuple<bool, string> result = Tuple.Create(false, string.Empty);
			if (string.IsNullOrWhiteSpace(argument))
			{
				return result;
			}
			argument = argument.Trim();
			foreach (string text in flagPrefixes)
			{
				if (argument.StartsWith(text, StringComparison.OrdinalIgnoreCase))
				{
					argument = argument.Substring(text.Length);
					argument = argument.Split(':', ' ').FirstOrDefault();
					result = Tuple.Create(true, argument);
					break;
				}
			}
			return result;
		}
	}
}

using Coding4Fun.VisualStudio.RemoteControl;
using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal class TelemetryManifestManager : TelemetryDisposableObject, ITelemetryManifestManager, IDisposable
	{
		private const int RemoteControlReadFileTelemetryFrequency = 6;

		private static readonly TimeSpan DownloadInterval = TimeSpan.FromMinutes(30.0);

		private static readonly TimeSpan ReadInterval = TimeSpan.FromMinutes(5.0);

		private static readonly TimeSpan ForceReadDelay = TimeSpan.FromSeconds(10.0);

		private readonly ITelemetryManifestParser manifestParser;

		private readonly ITelemetryScheduler scheduler;

		private readonly TelemetrySession mainSession;

		private IRemoteControlClient remoteControlClient;

		private ITelemetryManifestManagerSettings settings;

		private CancellationTokenSource tokenSource = new CancellationTokenSource();

		private bool isStarted;

		public bool ForcedReadManifest
		{
			get;
			private set;
		}

		internal TelemetryManifest CurrentManifest
		{
			get;
			private set;
		}

		public event EventHandler<TelemetryManifestEventArgs> UpdateTelemetryManifestStatusEvent;

		public TelemetryManifestManager(IRemoteControlClient theRemoteControlClient, ITelemetryManifestManagerSettings theSettings, ITelemetryManifestParser theManifestParser, ITelemetryScheduler theScheduler, TelemetrySession theMainSession)
		{
			CodeContract.RequiresArgumentNotNull<ITelemetryManifestParser>(theManifestParser, "theManifestParser");
			CodeContract.RequiresArgumentNotNull<ITelemetryScheduler>(theScheduler, "theScheduler");
			CodeContract.RequiresArgumentNotNull<TelemetrySession>(theMainSession, "theMainSession");
			manifestParser = theManifestParser;
			scheduler = theScheduler;
			scheduler.InitializeTimed(ReadInterval);
			mainSession = theMainSession;
			remoteControlClient = theRemoteControlClient;
			settings = theSettings;
			RemoteControlClient.TelemetryLogger2=((Action<string, IDictionary<string, object>, IDictionary<string, object>>)delegate(string eventName, IDictionary<string, object> properties, IDictionary<string, object> piiProperties)
			{
				TelemetryEvent telemetryEvent = new TelemetryEvent(eventName);
				DictionaryExtensions.AddRange<string, object>(telemetryEvent.Properties, properties, true);
				DictionaryExtensions.AddRange<string, object>(telemetryEvent.Properties, (IDictionary<string, object>)((IEnumerable<KeyValuePair<string, object>>)piiProperties).ToDictionary((Func<KeyValuePair<string, object>, string>)((KeyValuePair<string, object> p) => p.Key), (Func<KeyValuePair<string, object>, object>)((KeyValuePair<string, object> p) => new TelemetryPiiProperty(p.Value))), true);
				mainSession.PostEvent(telemetryEvent);
			});
		}

		public void Start(string hostName, bool isDisposing)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected O, but got Unknown
			if (!isStarted)
			{
				if (settings == null)
				{
					settings = new TelemetryManifestManagerSettings(hostName);
				}
				if (remoteControlClient == null)
				{
					remoteControlClient = (IRemoteControlClient)(object)new RemoteControlClient(settings.HostId, settings.BaseUrl, settings.RelativePath, (int)DownloadInterval.TotalMinutes, 60, 6);
				}
				if (!isDisposing)
				{
					CancellationToken token = tokenSource.Token;
					Func<Task> actionTask = async delegate
					{
						if (!token.IsCancellationRequested)
						{
							await Check((BehaviorOnStale)0, token).ConfigureAwait(false);
							if (!token.IsCancellationRequested && CurrentManifest == null)
							{
								await Check((BehaviorOnStale)2, token).ConfigureAwait(false);
							}
						}
					};
					scheduler.Schedule(actionTask, token);
					scheduler.ScheduleTimed((Func<Task>)Check);
				}
				isStarted = true;
			}
		}

		public bool ForceReadManifest()
		{
			if (CurrentManifest == null)
			{
				tokenSource.Cancel();
				ForcedReadManifest = true;
				CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
				if (!Check((BehaviorOnStale)0, cancellationTokenSource.Token).Wait(ForceReadDelay))
				{
					cancellationTokenSource.Cancel();
				}
			}
			return CurrentManifest != null;
		}

		internal async Task Check()
		{
			await Check((BehaviorOnStale)0).ConfigureAwait(false);
		}

		protected override void DisposeManagedResources()
		{
			if (isStarted)
			{
				if (!ForcedReadManifest)
				{
					tokenSource.Cancel();
				}
				((IDisposable)remoteControlClient).Dispose();
				scheduler.CancelTimed(true);
			}
		}

		private async Task Check(BehaviorOnStale staleBehavior, CancellationToken token = default(CancellationToken))
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				await LoadManifest(staleBehavior, token).ConfigureAwait(false);
			}
			catch (TelemetryManifestParserException ex)
			{
				if (!token.IsCancellationRequested)
				{
					List<string> list = new List<string>();
					for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
					{
						list.Add(innerException.Message);
					}
					OnUpdateTelemetryManifestStatusEvent(new TelemetryManifestEventArgs(null));
					InstrumentLoad(null, 0L, ex.Message, (list.Count > 0) ? StringExtensions.Join((IEnumerable<string>)list, ";") : null, 0.0);
				}
			}
			catch (Exception exceptionObject)
			{
				FaultEvent faultEvent = new FaultEvent("VS/Telemetry/InternalFault", $"LoadManifest ManifestManager.Check", exceptionObject)
				{
					PostThisEventToTelemetry = false
				};
				faultEvent.AddProcessDump(Process.GetCurrentProcess().Id);
				mainSession.PostEvent(faultEvent);
			}
		}

		private async Task LoadManifest(BehaviorOnStale staleBehavior, CancellationToken token = default(CancellationToken))
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Stopwatch watch = Stopwatch.StartNew();
			Tuple<TelemetryManifest, long> tuple = await ReadAndParseManifest(staleBehavior).ConfigureAwait(false);
			watch.Stop();
			if (!token.IsCancellationRequested && (CurrentManifest == null || (tuple.Item1 != null && !(CurrentManifest.Version == tuple.Item1.Version))))
			{
				string message = "Manifest is null";
				if (tuple.Item1 != null)
				{
					CurrentManifest = tuple.Item1;
					message = null;
				}
				OnUpdateTelemetryManifestStatusEvent(new TelemetryManifestEventArgs(tuple.Item1));
				InstrumentLoad(tuple.Item1, tuple.Item2, message, null, watch.ElapsedMilliseconds);
			}
		}

		private async Task<Tuple<TelemetryManifest, long>> ReadAndParseManifest(BehaviorOnStale staleBehavior)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Stream stream = await remoteControlClient.ReadFileAsync(staleBehavior).ConfigureAwait(false);
			if (stream == null)
			{
				return new Tuple<TelemetryManifest, long>(null, 0L);
			}
			using (StreamReader streamReader = new StreamReader(stream))
			{
				_ = 1;
				try
				{
					return new Tuple<TelemetryManifest, long>((TelemetryManifest)(await manifestParser.ParseAsync(streamReader).ConfigureAwait(false)), stream.Length);
				}
				catch (Exception ex)
				{
					if (ex is IOException || ex is ThreadAbortException)
					{
						return new Tuple<TelemetryManifest, long>(null, 0L);
					}
					throw;
				}
			}
		}

		private void OnUpdateTelemetryManifestStatusEvent(TelemetryManifestEventArgs e)
		{
			this.UpdateTelemetryManifestStatusEvent?.Invoke(this, e);
		}

		private void InstrumentLoad(TelemetryManifest telemetryManifest, long streamSize, string message, string errorDetails, double duration)
		{
			if (mainSession.IsSessionCloned)
			{
				return;
			}
			bool flag = telemetryManifest != null;
			TelemetryEvent telemetryEvent = new TelemetryEvent("VS/TelemetryApi/Manifest/Load");
			telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.HostName"] = mainSession.HostName;
			telemetryEvent.Properties["VS.TelemetryApi.Manifest.Load.IsLoadSuccess"] = flag;
			if (streamSize > 0)
			{
				telemetryEvent.Properties["VS.TelemetryApi.Manifest.Load.StreamSize"] = streamSize;
			}
			if (flag)
			{
				if (telemetryManifest != null)
				{
					telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.Manifest.Version"] = telemetryManifest.Version;
					telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.Manifest.FormatVersion"] = 2u;
					if (telemetryManifest.InvalidRules.Any())
					{
						telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.Manifest.UnrecognizedRules"] = StringExtensions.Join(telemetryManifest.InvalidRules, ",");
					}
					telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.Manifest.UnrecognizedRules.Count"] = telemetryManifest.InvalidRules.Count();
					telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.Manifest.UnrecognizedActions.Count"] = telemetryManifest.InvalidActionCount;
					string text = StringExtensions.Join(from path in telemetryManifest.GetAllSamplings()
						where path.Sampling.IsSampleActive
						select path.FullName, ",");
					if (text != string.Empty)
					{
						telemetryEvent.Properties["VS.TelemetryApi.DynamicTelemetry.Manifest.EnabledSamplings"] = text;
					}
				}
				telemetryEvent.Properties["VS.TelemetryApi.Manifest.Load.Duration"] = duration;
			}
			else
			{
				telemetryEvent.Properties["VS.TelemetryApi.Manifest.Load.ErrorMessage"] = message;
				if (errorDetails != null)
				{
					telemetryEvent.Properties["VS.TelemetryApi.Manifest.Load.ErrorDetails"] = new TelemetryPiiProperty(errorDetails);
				}
			}
			mainSession.PostEvent(telemetryEvent);
		}
	}
}

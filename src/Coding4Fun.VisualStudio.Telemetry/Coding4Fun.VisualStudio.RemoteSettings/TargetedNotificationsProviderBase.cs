using Coding4Fun.VisualStudio.Experimentation;
using Coding4Fun.VisualStudio.Telemetry;
using Coding4Fun.VisualStudio.Telemetry.Notification;
using Coding4Fun.VisualStudio.Telemetry.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal abstract class TargetedNotificationsProviderBase : RemoteSettingsProviderBase, IRemoteSettingsProvider, ISettingsCollection, IDisposable
	{
		internal const string TargetedNotificationsTelemetryEventPath = "VS/Core/TargetedNotifications/";

		internal const string TargetedNotificationsTelemetryPropertyPath = "VS.Core.TargetedNotifications.";

		internal readonly IDictionary<string, ActionCategory> ActionCategories = new Dictionary<string, ActionCategory>(StringComparer.OrdinalIgnoreCase);

		protected const string RemoteSettingsActionPath = "VS\\Core\\RemoteSettings";

		protected readonly bool useCache;

		protected readonly bool enforceCourtesy;

		protected readonly int cacheTimeoutMs;

		protected readonly TimeSpan serviceQueryLoopTimeSpan;

		protected readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		protected readonly IRemoteSettingsTelemetry remoteSettingsTelemetry;

		protected readonly ITargetedNotificationsTelemetry targetedNotificationsTelemetry;

		protected readonly TargetedNotificationsCacheProvider notificationAndCourtesyCache;

		protected Stopwatch apiTimer;

		protected int queryIteration;

		private const string RemoteSettingsTelemetryEventPath = "VS/Core/RemoteSettings/Targeted";

		private const string RemoteSettingsTelemetryPropertyPath = "VS.Core.RemoteSettings.Targeted";

		private const string TargetedNotificationsCacheableStorageCollectionPath = "TargetedNotifications";

		private readonly IDictionary<string, Dictionary<string, ActionResponse>> tnActions = new Dictionary<string, Dictionary<string, ActionResponse>>(StringComparer.OrdinalIgnoreCase);

		private readonly SemaphoreSlim actionsAndCategoriesLock = new SemaphoreSlim(1, 1);

		private readonly IDictionary<string, List<int>> tnSubscriptionIds = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);

		private readonly IDictionary<string, Dictionary<Type, IList>> tnSubscriptionCallbacks = new Dictionary<string, Dictionary<Type, IList>>(StringComparer.OrdinalIgnoreCase);

		private readonly object subscriptionLockObject = new object();

		private readonly IRemoteSettingsStorageHandler cacheableStorageHandler;

		private readonly IRemoteSettingsStorageHandler liveStorageHandler;

		private readonly IRemoteSettingsParser remoteSettingsParser;

		private readonly IExperimentationService experimentationService;

		private readonly ITelemetryNotificationService telemetryNotificationService;

		public TargetedNotificationsProviderBase(IRemoteSettingsStorageHandler cacheableStorageHandler, RemoteSettingsInitializer initializer)
			: base(cacheableStorageHandler, initializer.RemoteSettingsLogger)
		{
			this.cacheableStorageHandler = cacheableStorageHandler;
			useCache = GetValueOrDefaultFromCacheableStorage("TargetedNotifications", "UseCache", true);
			enforceCourtesy = GetValueOrDefaultFromCacheableStorage("TargetedNotifications", "EnforceCourtesy", true);
			cacheTimeoutMs = GetValueOrDefaultFromCacheableStorage("TargetedNotifications", "CacheTimeoutMs", 750);
			serviceQueryLoopTimeSpan = TimeSpan.FromMinutes(GetValueOrDefaultFromCacheableStorage("TargetedNotifications", "ServiceQueryLoopMinutes", 1440));
			liveStorageHandler = initializer.LiveRemoteSettingsStorageHandlerFactory();
			remoteSettingsParser = initializer.RemoteSettingsParser;
			remoteSettingsTelemetry = initializer.Telemetry;
			targetedNotificationsTelemetry = initializer.TargetedNotificationsTelemetry;
			experimentationService = initializer.ExperimentationService;
			telemetryNotificationService = initializer.TelemetryNotificationService;
			notificationAndCourtesyCache = new TargetedNotificationsCacheProvider(enforceCourtesy, this, initializer);
		}

		public override Task<GroupedRemoteSettings> Start()
		{
			Interlocked.Exchange(ref startTask, Task.Run(async delegate
			{
				GroupedRemoteSettings result = null;
				queryIteration++;
				ActionResponseBag actionResponseBag = await GetTargetedNotificationActionsAsync().ConfigureAwait(false);
				if (actionResponseBag != null)
				{
					ProcessActionResponseBag(actionResponseBag);
					result = await ProcessRemoteSettingsFromTargetedNotificationsAsync().ConfigureAwait(false);
				}
				StartAgainAfter(serviceQueryLoopTimeSpan);
				return result;
			}));
			return startTask;
		}

		internal async void StartAgainAfter(TimeSpan delayTime)
		{
			if (delayTime > TimeSpan.Zero && !cancellationTokenSource.IsCancellationRequested)
			{
				try
				{
					await Task.Delay(delayTime, cancellationTokenSource.Token).ConfigureAwait(false);
					Start();
				}
				catch (TaskCanceledException)
				{
				}
			}
		}

		public override async Task<IEnumerable<ActionWrapper<T>>> GetActionsAsync<T>(string actionPath)
		{
			RequiresNotDisposed();
			await startTask.ConfigureAwait(false);
			IEnumerable<LoggingContext<ActionWrapper<T>>> source;
			try
			{
				await actionsAndCategoriesLock.WaitAsync().ConfigureAwait(false);
				source = await GetActionsInternalAsync<T>(actionPath, true).ConfigureAwait(false);
			}
			finally
			{
				actionsAndCategoriesLock.Release();
			}
			return source.Select((LoggingContext<ActionWrapper<T>> x) => x.Value);
		}

		public override async void SubscribeActions<T>(string actionPath, Action<ActionWrapper<T>> callback)
		{
			if (!base.IsDisposed)
			{
				await startTask.ConfigureAwait(false);
				try
				{
					await actionsAndCategoriesLock.WaitAsync().ConfigureAwait(false);
					await SubscribeActionsInternal(actionPath, callback).ConfigureAwait(false);
				}
				finally
				{
					actionsAndCategoriesLock.Release();
				}
			}
		}

		public override void UnsubscribeActions(string actionPath)
		{
			RequiresNotDisposed();
			actionPath = actionPath.NormalizePath();
			lock (subscriptionLockObject)
			{
				if (tnSubscriptionIds.ContainsKey(actionPath))
				{
					foreach (int item in tnSubscriptionIds[actionPath])
					{
						telemetryNotificationService.Unsubscribe(item);
					}
					tnSubscriptionIds.Remove(actionPath);
				}
			}
		}

		protected override void DisposeManagedResources()
		{
			cancellationTokenSource.Cancel();
			lock (subscriptionLockObject)
			{
				foreach (string key in tnSubscriptionIds.Keys)
				{
					UnsubscribeActions(key);
				}
				tnSubscriptionIds.Clear();
				tnSubscriptionCallbacks.Clear();
			}
		}

		protected abstract Task<ActionResponseBag> GetTargetedNotificationActionsAsync();

		private void ProcessActionResponseBag(ActionResponseBag response)
		{
			logger.LogInfo($"Received {response.Actions.Count()} actions from {Name}");
			List<string> list;
			try
			{
				actionsAndCategoriesLock.Wait();
				list = tnActions.Values.SelectMany((Dictionary<string, ActionResponse> x) => x.Keys).ToList();
				foreach (ActionResponse action in response.Actions)
				{
					if (!tnActions.TryGetValue(action.ActionPath, out Dictionary<string, ActionResponse> value))
					{
						value = new Dictionary<string, ActionResponse>(StringComparer.OrdinalIgnoreCase);
						tnActions[action.ActionPath] = value;
					}
					if (action.RuleId != null)
					{
						value[action.RuleId] = action;
					}
					else
					{
						logger.LogInfo("Skipping action that has no RuleId " + action.ActionPath + " from " + Name);
					}
				}
				foreach (ActionCategory category in response.Categories)
				{
					ActionCategories[category.CategoryId] = category;
				}
			}
			finally
			{
				actionsAndCategoriesLock.Release();
			}
			lock (subscriptionLockObject)
			{
				try
				{
					foreach (string key in tnSubscriptionCallbacks.Keys)
					{
						foreach (Type key2 in tnSubscriptionCallbacks[key].Keys)
						{
							MethodInfo methodInfo = typeof(TargetedNotificationsProviderBase).GetMethod("SubscribeActionsInternal", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(key2);
							foreach (object item in tnSubscriptionCallbacks[key][key2])
							{
								methodInfo.Invoke(this, new object[3]
								{
									key,
									item,
									list
								});
							}
						}
					}
				}
				catch (Exception exception)
				{
					string eventName = "VS/Core/TargetedNotifications/ReSubscribeFailure";
					targetedNotificationsTelemetry.PostCriticalFault(eventName, "failure", exception);
				}
			}
			string eventName2 = "VS/Core/TargetedNotifications/ActionsReceived";
			IEnumerable<ActionResponse> source = response.Actions ?? Enumerable.Empty<ActionResponse>();
			Dictionary<string, IEnumerable<string>> val = (from x in source
				group x by x.ActionPath).ToDictionary((IGrouping<string, ActionResponse> x) => x.Key, (IGrouping<string, ActionResponse> x) => x.Select((ActionResponse y) => y.RuleId));
			List<string> list2 = ActionCategories.Keys.ToList();
			Dictionary<string, object> additionalProperties = new Dictionary<string, object>
			{
				{
					"VS.Core.TargetedNotifications.ProviderName",
					Name
				},
				{
					"VS.Core.TargetedNotifications.Actions",
					new TelemetryComplexProperty(val)
				},
				{
					"VS.Core.TargetedNotifications.ActionCount",
					source.Count()
				},
				{
					"VS.Core.TargetedNotifications.Categories",
					new TelemetryComplexProperty(list2)
				},
				{
					"VS.Core.TargetedNotifications.CategoryCount",
					list2.Count
				},
				{
					"VS.Core.TargetedNotifications.ApiResponseMs",
					apiTimer?.ElapsedMilliseconds
				},
				{
					"VS.Core.TargetedNotifications.Iteration",
					queryIteration
				}
			};
			targetedNotificationsTelemetry.PostSuccessfulOperation(eventName2, additionalProperties);
		}

		private async Task<GroupedRemoteSettings> ProcessRemoteSettingsFromTargetedNotificationsAsync()
		{
			IEnumerable<LoggingContext<ActionWrapper<JObject>>> source = await GetActionsInternalAsync<JObject>("VS\\Core\\RemoteSettings", false).ConfigureAwait(false);
			if (source.Any())
			{
				List<GroupedRemoteSettings> list = new List<GroupedRemoteSettings>();
				List<Tuple<ActionWrapper<JObject>, DeserializedRemoteSettings>> list2 = new List<Tuple<ActionWrapper<JObject>, DeserializedRemoteSettings>>();
				foreach (LoggingContext<ActionWrapper<JObject>> item in source.OrderBy((LoggingContext<ActionWrapper<JObject>> x) => x.Value.Precedence))
				{
					DeserializedRemoteSettings deserializedRemoteSettings = remoteSettingsParser.TryParseFromJObject(item.Value.Action, string.IsNullOrEmpty(item.Value.FlightName) ? null : ("Flight." + item.Value.FlightName));
					if (deserializedRemoteSettings.Successful)
					{
						list.Add(new GroupedRemoteSettings(deserializedRemoteSettings, Name + "-" + item.Context));
					}
					else
					{
						logger.LogError("Error deserializing TN rule " + item.Context + ": " + deserializedRemoteSettings.Error);
						list2.Add(Tuple.Create(item.Value, deserializedRemoteSettings));
					}
				}
				if (list2.Any())
				{
					string name = "VS/Core/RemoteSettings/TargetedParseSettings";
					Dictionary<string, object> dictionary = new Dictionary<string, object>();
					dictionary["VS.Core.RemoteSettings.TargetedRuleIds"] = string.Join(",", list2.Select((Tuple<ActionWrapper<JObject>, DeserializedRemoteSettings> x) => x.Item1.RuleId));
					dictionary["VS.Core.RemoteSettings.TargetedErrors"] = string.Join(",", list2.Select((Tuple<ActionWrapper<JObject>, DeserializedRemoteSettings> x) => x.Item2.Error));
					remoteSettingsTelemetry.PostEvent(name, dictionary);
				}
				if (list.Any())
				{
					GroupedRemoteSettings groupedRemoteSettings = list.Aggregate(delegate(GroupedRemoteSettings a, GroupedRemoteSettings b)
					{
						a.Merge(b, logger);
						return a;
					});
					liveStorageHandler.SaveSettings(groupedRemoteSettings);
					Interlocked.Exchange(ref currentStorageHandler, liveStorageHandler);
					cacheableStorageHandler.DeleteAllSettings();
					cacheableStorageHandler.SaveSettings(groupedRemoteSettings);
					return groupedRemoteSettings;
				}
			}
			else
			{
				Interlocked.Exchange(ref currentStorageHandler, liveStorageHandler);
				cacheableStorageHandler.DeleteAllSettings();
			}
			return null;
		}

		private async Task<IEnumerable<LoggingContext<ActionWrapper<T>>>> GetActionsInternalAsync<T>(string actionPath, bool shouldCheckFlight)
		{
			actionPath = actionPath.NormalizePath();
			IEnumerable<ActionResponse> filteredActionsForPath = new List<ActionResponse>();
			List<LoggingContext<ActionWrapper<T>>> returnedActions = new List<LoggingContext<ActionWrapper<T>>>();
			if (tnActions.TryGetValue(actionPath, out Dictionary<string, ActionResponse> value))
			{
				foreach (ActionResponse actionResponse in value.Values)
				{
					if (string.IsNullOrWhiteSpace(actionResponse.TriggerJson))
					{
						bool flag = true;
						if (shouldCheckFlight && !string.IsNullOrEmpty(actionResponse.FlightName))
						{
							try
							{
								flag = await experimentationService.IsFlightEnabledAsync(actionResponse.FlightName, cancellationTokenSource.Token).ConfigureAwait(false);
							}
							catch
							{
								flag = false;
							}
							logger.LogVerbose($"{Name}-{actionResponse} depends on flight {actionResponse.FlightName}, isFlightEnabled: {flag}");
						}
						if (flag)
						{
							(filteredActionsForPath as List<ActionResponse>).Add(actionResponse);
						}
					}
				}
				if (useCache)
				{
					filteredActionsForPath = notificationAndCourtesyCache.GetSendableActionsFromSet(filteredActionsForPath, cacheTimeoutMs);
				}
				foreach (ActionResponse item in filteredActionsForPath)
				{
					try
					{
						returnedActions.Add(new LoggingContext<ActionWrapper<T>>(item.ToString(), item.AsTypedAction<T>()));
					}
					catch (TargetedNotificationsException exception)
					{
						string eventName = "VS/Core/TargetedNotifications/ActionJsonParseFailure";
						string text = $"Error handling {Name}-{item}";
						Dictionary<string, object> additionalProperties = new Dictionary<string, object>
						{
							{
								"VS.Core.TargetedNotifications.RuleId",
								item.RuleId
							},
							{
								"VS.Core.TargetedNotifications.FlightName",
								item.FlightName
							}
						};
						logger.LogError(text, exception);
						targetedNotificationsTelemetry.PostCriticalFault(eventName, text, exception, additionalProperties);
					}
				}
			}
			logger.LogVerbose($"Got {returnedActions.Count()} actions for path {actionPath} from provider {Name}");
			return returnedActions;
		}

		private bool IsValidTriggerConfiguration(Dictionary<string, ITelemetryEventMatch> triggers, Dictionary<string, ActionTriggerOptions> options)
		{
			if (triggers == null || options == null || triggers.Count == 0 || !triggers.ContainsKey("start") || triggers.Count != options.Count)
			{
				return false;
			}
			foreach (string key in triggers.Keys)
			{
				if (!options.ContainsKey(key))
				{
					return false;
				}
				if (string.Equals(key, "start", StringComparison.OrdinalIgnoreCase) && options[key].TriggerOnSubscribe)
				{
					if (triggers[key].GetType() != typeof(TelemetryManifestInvalidMatchItem))
					{
						return false;
					}
				}
				else if (triggers[key].GetType() == typeof(TelemetryManifestInvalidMatchItem) || options[key].TriggerOnSubscribe)
				{
					return false;
				}
			}
			return true;
		}

		private async Task SubscribeActionsInternal<T>(string actionPath, Action<ActionWrapper<T>> callback, IEnumerable<string> previouslySubscribedRuleIds = null)
		{
			actionPath = actionPath.NormalizePath();
			if (previouslySubscribedRuleIds == null)
			{
				lock (subscriptionLockObject)
				{
					if (!tnSubscriptionCallbacks.ContainsKey(actionPath))
					{
						tnSubscriptionCallbacks[actionPath] = new Dictionary<Type, IList>();
					}
					if (!tnSubscriptionCallbacks[actionPath].ContainsKey(typeof(T)))
					{
						tnSubscriptionCallbacks[actionPath][typeof(T)] = new List<Action<ActionWrapper<T>>>();
					}
					tnSubscriptionCallbacks[actionPath][typeof(T)].Add(callback);
				}
			}
			new List<Tuple<ActionResponse, string>>();
			int returnedActionCount = 0;
			if (tnActions.TryGetValue(actionPath, out Dictionary<string, ActionResponse> value))
			{
				foreach (ActionResponse actionResponse in value.Values)
				{
					try
					{
						if ((previouslySubscribedRuleIds == null || !previouslySubscribedRuleIds.Contains(actionResponse.RuleId)) && !string.IsNullOrWhiteSpace(actionResponse.TriggerJson))
						{
							bool flag = true;
							if (!string.IsNullOrEmpty(actionResponse.FlightName))
							{
								try
								{
									flag = await experimentationService.IsFlightEnabledAsync(actionResponse.FlightName, cancellationTokenSource.Token).ConfigureAwait(false);
								}
								catch
								{
									flag = false;
								}
								logger.LogVerbose($"{Name}-{actionResponse} depends on flight {actionResponse.FlightName}, isFlightEnabled: {flag}");
							}
							if (flag)
							{
								Dictionary<string, ActionTriggerOptions> triggerOptions = actionResponse.GetTriggerOptions();
								Dictionary<string, ITelemetryEventMatch> triggers = actionResponse.GetTriggers();
								if (!IsValidTriggerConfiguration(triggers, triggerOptions))
								{
									string eventName = "VS/Core/TargetedNotifications/TriggerJsonInvalid";
									string description = $"Error handling {Name}-{actionResponse}";
									Dictionary<string, object> additionalProperties = new Dictionary<string, object>
									{
										{
											"VS.Core.TargetedNotifications.RuleId",
											actionResponse.RuleId
										},
										{
											"VS.Core.TargetedNotifications.FlightName",
											actionResponse.FlightName
										}
									};
									targetedNotificationsTelemetry.PostCriticalFault(eventName, description, null, additionalProperties);
								}
								else
								{
									object triggerLockObject = new object();
									bool startTriggerInvoked = false;
									ActionWrapper<T> baseActionWrapper = (ActionWrapper<T>)(object)actionResponse.AsTypedAction<T>();
									Dictionary<string, int> triggerSubscriptions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
									foreach (string triggerName in triggers.Keys)
									{
										bool triggerInvoked = false;
										ActionWrapper<T> actionWrapper = default(ActionWrapper<T>);
										Action<TelemetryEvent> wrappedCallback = delegate(TelemetryEvent telemetryEvent)
										{
											lock (triggerLockObject)
											{
												bool flag2 = string.Equals(triggerName, "start", StringComparison.OrdinalIgnoreCase);
												if (flag2 && !startTriggerInvoked && useCache)
												{
													try
													{
														actionsAndCategoriesLock.Wait();
														if (notificationAndCourtesyCache.GetSendableAction(actionResponse, cacheTimeoutMs) == null)
														{
															return;
														}
													}
													finally
													{
														actionsAndCategoriesLock.Release();
													}
												}
												if ((startTriggerInvoked | flag2) && (triggerOptions[triggerName].TriggerAlways || !triggerInvoked))
												{
													triggerInvoked = true;
													actionWrapper = (ActionWrapper<T>)(object)((ActionWrapper<T>)(object)baseActionWrapper).WithSubscriptionDetails(new ActionSubscriptionDetails
													{
														TriggerName = triggerName,
														TelemetryEvent = telemetryEvent,
														NotificationService = telemetryNotificationService,
														TriggerSubscriptions = triggerSubscriptions,
														TriggerAlways = triggerOptions[triggerName].TriggerAlways,
														TriggerOnSubscribe = triggerOptions[triggerName].TriggerOnSubscribe,
														TriggerLockObject = triggerLockObject
													});
													if (!((ActionWrapper<T>)(object)actionWrapper).Subscription.TriggerAlways && triggerSubscriptions.ContainsKey(triggerName) && !((ActionWrapper<T>)(object)actionWrapper).Subscription.TriggerOnSubscribe)
													{
														telemetryNotificationService.Unsubscribe(triggerSubscriptions[triggerName]);
														triggerSubscriptions.Remove(triggerName);
													}
													string eventName3 = "VS/Core/TargetedNotifications/SubscriptionTriggered";
													Dictionary<string, object> additionalProperties3 = new Dictionary<string, object>
													{
														{
															"VS.Core.TargetedNotifications.RuleId",
															actionResponse.RuleId
														},
														{
															"VS.Core.TargetedNotifications.TriggerName",
															triggerName
														}
													};
													targetedNotificationsTelemetry.PostSuccessfulOperation(eventName3, additionalProperties3);
													Task.Run(delegate
													{
														callback((ActionWrapper<T>)(object)actionWrapper);
													});
													if (flag2)
													{
														startTriggerInvoked = true;
													}
												}
											}
										};
										if (triggers[triggerName].GetType() != typeof(TelemetryManifestInvalidMatchItem) && !triggerOptions[triggerName].TriggerOnSubscribe)
										{
											lock (triggerLockObject)
											{
												triggerSubscriptions[triggerName] = telemetryNotificationService.Subscribe(triggers[triggerName], wrappedCallback, false);
												lock (subscriptionLockObject)
												{
													if (!tnSubscriptionIds.ContainsKey(actionPath))
													{
														tnSubscriptionIds[actionPath] = new List<int>();
													}
													tnSubscriptionIds[actionPath].Add(triggerSubscriptions[triggerName]);
												}
											}
										}
										else
										{
											if (!(triggerName == "start") || !triggerOptions[triggerName].TriggerOnSubscribe)
											{
												throw new TargetedNotificationsException("Invalid trigger: " + triggerName);
											}
											if (previouslySubscribedRuleIds == null)
											{
												Task.Run(delegate
												{
													wrappedCallback(null);
												});
											}
										}
									}
								}
							}
						}
					}
					catch (TargetedNotificationsException exception)
					{
						string eventName2 = "VS/Core/TargetedNotifications/ActionJsonParseFailure";
						string text = $"Error handling {Name}-{actionResponse}";
						Dictionary<string, object> additionalProperties2 = new Dictionary<string, object>
						{
							{
								"VS.Core.TargetedNotifications.RuleId",
								actionResponse.RuleId
							},
							{
								"VS.Core.TargetedNotifications.FlightName",
								actionResponse.FlightName
							}
						};
						logger.LogError(text, exception);
						targetedNotificationsTelemetry.PostCriticalFault(eventName2, text, exception, additionalProperties2);
					}
				}
			}
			logger.LogVerbose($"Subscribe got {returnedActionCount} actions for path {actionPath} from provider {Name}");
		}

		private T GetValueOrDefaultFromCacheableStorage<T>(string collectionPath, string key, T defaultValue)
		{
			if (cacheableStorageHandler.TryGetValue(collectionPath, key, out T value))
			{
				return value;
			}
			return defaultValue;
		}
	}
}

using Coding4Fun.VisualStudio.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class TargetedNotificationsCacheProvider
	{
		private readonly ITargetedNotificationsTelemetry telemetry;

		private readonly TargetedNotificationsProviderBase tnProvider;

		private readonly bool enforceCourtesy;

		private readonly TimeSpan defaultMaxWaitTimeSpan = TimeSpan.FromDays(14.0);

		private bool responseUsesCachedRules;

		private ITargetedNotificationsCacheStorageProvider Storage
		{
			get;
			set;
		}

		public TargetedNotificationsCacheProvider(bool enforceCourtesy, TargetedNotificationsProviderBase tnProvider, RemoteSettingsInitializer initializer)
		{
			this.enforceCourtesy = enforceCourtesy;
			this.tnProvider = tnProvider;
			Storage = initializer.TargetedNotificationsCacheStorage;
			telemetry = initializer.TargetedNotificationsTelemetry;
			responseUsesCachedRules = false;
		}

		/// <summary>
		/// Called when a new ActionResponseBag is received from the
		/// service. This merges that new data into the existing cache
		/// </summary>
		/// <param name="newResponse">An ActionResponseBag received from the Azure API</param>
		/// <param name="previouslyCachedRuleIds">Set of rule IDs that were previously read from the cache and should not be written back</param>
		/// <param name="timeoutMs">Maximum time to wait, in milliseconds, for the cache lock. Leave null for infinite.</param>
		public void MergeNewResponse(ActionResponseBag newResponse, IEnumerable<string> previouslyCachedRuleIds, int? timeoutMs = null)
		{
			if (Storage.Lock(timeoutMs))
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				try
				{
					DateTime utcNow = DateTime.UtcNow;
					bool flag = false;
					CachedTargetedNotifications localCacheCopy = Storage.GetLocalCacheCopy();
					List<string> list = new List<string>();
					List<string> list2 = new List<string>();
					foreach (ActionResponse action in newResponse.Actions)
					{
						if (action.SendAlways)
						{
							if (localCacheCopy.Actions.Remove(action.RuleId))
							{
								flag = true;
							}
						}
						else if (!previouslyCachedRuleIds.Contains(action.RuleId))
						{
							localCacheCopy.Actions[action.RuleId] = new CachedActionResponseTime
							{
								CachedTime = (localCacheCopy.Actions.ContainsKey(action.RuleId) ? localCacheCopy.Actions[action.RuleId].CachedTime : utcNow),
								MaxWaitTimeSpan = ((action.MaxWaitTimeSpan == null) ? defaultMaxWaitTimeSpan : TimeSpan.Parse(action.MaxWaitTimeSpan))
							};
							list.Add(action.RuleId);
							flag = true;
						}
					}
					string[] array = localCacheCopy.Actions.Keys.ToArray();
					foreach (string text in array)
					{
						CachedActionResponseTime cachedActionResponseTime = localCacheCopy.Actions[text];
						if (utcNow >= cachedActionResponseTime.CachedTime.Add(cachedActionResponseTime.MaxWaitTimeSpan))
						{
							localCacheCopy.Actions.Remove(text);
							list2.Add(text);
							flag = true;
						}
					}
					foreach (ActionCategory category in newResponse.Categories)
					{
						if (localCacheCopy.Categories.ContainsKey(category.CategoryId))
						{
							TimeSpan timeSpan = TimeSpan.Parse(category.WaitTimeSpan);
							if (localCacheCopy.Categories[category.CategoryId].WaitTimeSpan != timeSpan)
							{
								localCacheCopy.Categories[category.CategoryId].WaitTimeSpan = timeSpan;
								flag = true;
							}
						}
					}
					array = localCacheCopy.Categories.Keys.ToArray();
					foreach (string key in array)
					{
						CachedActionCategoryTime cachedActionCategoryTime = localCacheCopy.Categories[key];
						if (utcNow >= cachedActionCategoryTime.LastSent.Add(cachedActionCategoryTime.WaitTimeSpan))
						{
							localCacheCopy.Categories.Remove(key);
							flag = true;
						}
					}
					if (flag)
					{
						Storage.SetLocalCache(localCacheCopy);
					}
					if (list.Count > 0)
					{
						responseUsesCachedRules = true;
					}
					stopwatch.Stop();
					if (list.Count > 0 || list2.Count > 0 || localCacheCopy.Actions.Count > 0 || localCacheCopy.Categories.Count > 0)
					{
						string eventName = "VS/Core/TargetedNotifications/CacheMerged";
						Dictionary<string, object> additionalProperties = new Dictionary<string, object>
						{
							{
								"VS.Core.TargetedNotifications.AddedActions",
								new TelemetryComplexProperty(list)
							},
							{
								"VS.Core.TargetedNotifications.AddedActionsCount",
								list.Count
							},
							{
								"VS.Core.TargetedNotifications.ExpiredActions",
								new TelemetryComplexProperty(list2)
							},
							{
								"VS.Core.TargetedNotifications.ExpiredActionsCount",
								list2.Count
							},
							{
								"VS.Core.TargetedNotifications.CachedActions",
								new TelemetryComplexProperty(localCacheCopy.Actions.Keys.ToList())
							},
							{
								"VS.Core.TargetedNotifications.CachedActionsCount",
								localCacheCopy.Actions.Count
							},
							{
								"VS.Core.TargetedNotifications.CachedCategories",
								new TelemetryComplexProperty(localCacheCopy.Categories.Keys.ToList())
							},
							{
								"VS.Core.TargetedNotifications.CachedCategoriesCount",
								localCacheCopy.Categories.Count
							},
							{
								"VS.Core.TargetedNotifications.DurationMs",
								stopwatch.ElapsedMilliseconds
							}
						};
						telemetry.PostSuccessfulOperation(eventName, additionalProperties);
					}
				}
				catch (Exception exception)
				{
					stopwatch.Stop();
					string eventName2 = "VS/Core/TargetedNotifications/CacheMergeFailure";
					string[] array2 = (from a in newResponse.Actions
						where !a.SendAlways
						select a.RuleId).Except(previouslyCachedRuleIds).ToArray();
					Dictionary<string, object> additionalProperties2 = new Dictionary<string, object>
					{
						{
							"VS.Core.TargetedNotifications.SendOnceActions",
							new TelemetryComplexProperty(array2)
						},
						{
							"VS.Core.TargetedNotifications.SendOnceActionsCount",
							array2.Length
						},
						{
							"VS.Core.TargetedNotifications.DurationMs",
							stopwatch.ElapsedMilliseconds
						}
					};
					telemetry.PostCriticalFault(eventName2, "Failed to merge new response with cache", exception, additionalProperties2);
				}
				finally
				{
					Storage.Unlock();
				}
			}
			else
			{
				string eventName3 = "VS/Core/TargetedNotifications/CacheLockTimeout";
				Dictionary<string, object> additionalProperties3 = new Dictionary<string, object>
				{
					{
						"VS.Core.TargetedNotifications.Operation",
						"MergeNewResponse"
					},
					{
						"VS.Core.TargetedNotifications.TimeoutMs",
						timeoutMs
					}
				};
				telemetry.PostDiagnosticFault(eventName3, "Timeout acquiring cache lock", null, additionalProperties3);
			}
		}

		/// <summary>
		/// Gets all rule IDs currently stored in the local cache
		/// </summary>
		/// <param name="timeoutMs">Maximum time to wait, in milliseconds, for the cache lock. Leave null for infinite.</param>
		/// <returns>An IEnumerable of the rule IDs as strings. The IEnumerable will be empty of the timeout expires.</returns>
		public IEnumerable<string> GetAllCachedRuleIds(int? timeoutMs = null)
		{
			if (Storage.Lock(timeoutMs))
			{
				try
				{
					CachedTargetedNotifications localCacheCopy = Storage.GetLocalCacheCopy();
					if (localCacheCopy.Actions.Keys.Count > 0)
					{
						responseUsesCachedRules = true;
					}
					return localCacheCopy.Actions.Keys;
				}
				catch (Exception exception)
				{
					string eventName = "VS/Core/TargetedNotifications/CacheFailure";
					telemetry.PostCriticalFault(eventName, "Failed to get rule ids from cache", exception);
					return Enumerable.Empty<string>();
				}
				finally
				{
					Storage.Unlock();
				}
			}
			string eventName2 = "VS/Core/TargetedNotifications/CacheLockTimeout";
			Dictionary<string, object> additionalProperties = new Dictionary<string, object>
			{
				{
					"VS.Core.TargetedNotifications.Operation",
					"GetAllCachedRuleIds"
				},
				{
					"VS.Core.TargetedNotifications.TimeoutMs",
					timeoutMs
				}
			};
			telemetry.PostDiagnosticFault(eventName2, "Timeout acquiring cache lock", null, additionalProperties);
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Given an action, determines if that action can be sent now,
		/// meaning it isn't SendOnce and already sent by another instance
		/// and any Category courtesy WaitTimeSpans are expired.
		///
		/// This function updates cache state to indicate any returned action
		/// has been sent before returning it.
		/// </summary>
		/// <param name="action">The single action to check</param>
		/// <param name="timeoutMs">Milliseconds to wait for cache access. Leave null for infinite.</param>
		/// <returns>The action, if it is safe to sent to a feature, or null if it is not or the call timed out.</returns>
		public ActionResponse GetSendableAction(ActionResponse action, int? timeoutMs = null)
		{
			return GetSendableActionsFromSet(Enumerable.Repeat(action, 1), timeoutMs).FirstOrDefault();
		}

		/// <summary>
		/// Given a set of actions, determines which of them can be sent now,
		/// meaning they aren't SendOnce and already sent by another instance
		/// and any Category courtesy WaitTimeSpans have expired.
		///
		/// This function updates cache state to indicate any returned actions
		/// have been sent before returning them.
		/// </summary>
		/// <param name="actions">IEnumerable of ActionResponses to check against</param>
		/// <param name="timeoutMs">Milliseconds to wait for cache access. Leave null for infinite.</param>
		/// <returns>A subset of the given actions that are sendable.</returns>
		public IEnumerable<ActionResponse> GetSendableActionsFromSet(IEnumerable<ActionResponse> actions, int? timeoutMs = null)
		{
			if (!actions.Any((ActionResponse a) => (a.Categories != null && a.Categories.Count > 0) || (responseUsesCachedRules && !a.SendAlways)))
			{
				return actions;
			}
			Func<IEnumerable<ActionResponse>> func = () => actions.Where((ActionResponse a) => (a.Categories == null || a.Categories.Count == 0) && (!responseUsesCachedRules || a.SendAlways));
			if (Storage.Lock(timeoutMs))
			{
				try
				{
					bool flag = false;
					CachedTargetedNotifications localCacheCopy = Storage.GetLocalCacheCopy();
					List<ActionResponse> list = new List<ActionResponse>();
					foreach (ActionResponse action in actions)
					{
						if (action.SendAlways || !responseUsesCachedRules || localCacheCopy.Actions.ContainsKey(action.RuleId))
						{
							List<string> list2 = new List<string>();
							DateTime utcNow = DateTime.UtcNow;
							TimeSpan t = TimeSpan.MinValue;
							if (action.Categories != null)
							{
								foreach (string category in action.Categories)
								{
									if (localCacheCopy.Categories.ContainsKey(category))
									{
										CachedActionCategoryTime cachedActionCategoryTime = localCacheCopy.Categories[category];
										DateTime dateTime = cachedActionCategoryTime.LastSent.Add(cachedActionCategoryTime.WaitTimeSpan);
										if (utcNow < dateTime)
										{
											list2.Add(category);
											TimeSpan timeSpan = dateTime - utcNow;
											if (timeSpan > t)
											{
												t = timeSpan;
											}
										}
									}
								}
							}
							if (list2.Count == 0 || !enforceCourtesy)
							{
								IEnumerable<string> categories = action.Categories;
								List<string> list3 = (categories ?? Enumerable.Empty<string>()).Where((string c) => !tnProvider.ActionCategories.ContainsKey(c)).ToList();
								if (list3.Count() == 0 || !enforceCourtesy)
								{
									if (action.Categories != null)
									{
										foreach (string category2 in action.Categories)
										{
											if (tnProvider.ActionCategories.ContainsKey(category2))
											{
												flag = true;
												localCacheCopy.Categories[category2] = new CachedActionCategoryTime
												{
													LastSent = utcNow,
													WaitTimeSpan = TimeSpan.Parse(tnProvider.ActionCategories[category2].WaitTimeSpan)
												};
											}
										}
									}
									if (!action.SendAlways)
									{
										flag = true;
										localCacheCopy.Actions.Remove(action.RuleId);
									}
									list.Add(action);
								}
								else
								{
									string eventName = "VS/Core/TargetedNotifications/MissingCategories";
									Dictionary<string, object> additionalProperties = new Dictionary<string, object>
									{
										{
											"VS.Core.TargetedNotifications.RuleId",
											action.RuleId
										},
										{
											"VS.Core.TargetedNotifications.MissingCategories",
											new TelemetryComplexProperty(list3)
										}
									};
									telemetry.PostCriticalFault(eventName, "Rule requires category information that was not provided. It can never be sent.", null, additionalProperties);
								}
							}
							else
							{
								string eventName2 = "VS/Core/TargetedNotifications/CourtesyDeniedAction";
								Dictionary<string, object> additionalProperties2 = new Dictionary<string, object>
								{
									{
										"VS.Core.TargetedNotifications.RuleId",
										action.RuleId
									},
									{
										"VS.Core.TargetedNotifications.Categories",
										new TelemetryComplexProperty(list2)
									},
									{
										"VS.Core.TargetedNotifications.MinimumWaitTimeSpan",
										t.ToString()
									}
								};
								telemetry.PostSuccessfulOperation(eventName2, additionalProperties2);
							}
						}
					}
					if (flag)
					{
						Storage.SetLocalCache(localCacheCopy);
					}
					return list;
				}
				catch (Exception exception)
				{
					IEnumerable<ActionResponse> enumerable = func();
					List<string> list4 = actions.Select((ActionResponse a) => a.RuleId).Except(enumerable.Select((ActionResponse a) => a.RuleId)).ToList();
					string eventName3 = "VS/Core/TargetedNotifications/CacheFailure";
					Dictionary<string, object> additionalProperties3 = new Dictionary<string, object>
					{
						{
							"VS.Core.TargetedNotifications.BlockedRuleCount",
							list4.Count
						},
						{
							"VS.Core.TargetedNotifications.BlockedRuleIds",
							new TelemetryComplexProperty(list4)
						}
					};
					telemetry.PostCriticalFault(eventName3, "Failed to get sendable actions", exception, additionalProperties3);
					return enumerable;
				}
				finally
				{
					Storage.Unlock();
				}
			}
			IEnumerable<ActionResponse> enumerable2 = func();
			List<string> list5 = actions.Select((ActionResponse a) => a.RuleId).Except(enumerable2.Select((ActionResponse a) => a.RuleId)).ToList();
			string eventName4 = "VS/Core/TargetedNotifications/CacheLockTimeout";
			Dictionary<string, object> additionalProperties4 = new Dictionary<string, object>
			{
				{
					"VS.Core.TargetedNotifications.Operation",
					"GetSendableActionsFromSet"
				},
				{
					"VS.Core.TargetedNotifications.TimeoutMs",
					timeoutMs
				},
				{
					"VS.Core.TargetedNotifications.BlockedRuleCount",
					list5.Count
				},
				{
					"VS.Core.TargetedNotifications.BlockedRuleIds",
					new TelemetryComplexProperty(list5)
				}
			};
			telemetry.PostDiagnosticFault(eventName4, "Timeout acquiring cache lock", null, additionalProperties4);
			return enumerable2;
		}
	}
}

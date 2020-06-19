using Coding4Fun.VisualStudio.Utilities.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Helper class used for parsing a settings file that was copied down from Azure.
	/// </summary>
	internal class RemoteSettingsParser : IRemoteSettingsParser
	{
		private struct PropertyEntry
		{
			public readonly string Path;

			public readonly JProperty JProperty;

			public PropertyEntry(string path, JProperty jProperty)
			{
				Path = path;
				JProperty = jProperty;
			}
		}

		internal static readonly string FileVersionNotFirstPropertyErrorMessage = "The FileVersion was not the first property in the remote settings stream.";

		internal static readonly string ChangesetIdNotSecondPropertyErrorMessage = "The ChangesetId was not the second property in the remote settings stream.";

		internal static readonly string TypeNotSupportedErrorMessageFormat = "Type {0} not supported.";

		internal static readonly string ScopesWasNotObjectErrorMessage = "Scopes is not of type object";

		internal static readonly string ScopeWasNotStringErrorMessage = "A scope was not of type string";

		internal static readonly string InvalidJsonErrorMessage = "The remote settings stream was not a valid json document.";

		internal static readonly string UnhandledExceptionErrorMessageFormat = "An unhandled exception occurred while parsing the remote settings json document. Exception:\r\n{0}";

		private readonly IRemoteSettingsValidator remoteSettingsValidator;

		public RemoteSettingsParser(IRemoteSettingsValidator remoteSettingsValidator)
		{
			CodeContract.RequiresArgumentNotNull<IRemoteSettingsValidator>(remoteSettingsValidator, "remoteSettingsValidator");
			this.remoteSettingsValidator = remoteSettingsValidator;
		}

		/// <summary>
		/// Deserializes the contents of a remote settings file.
		/// </summary>
		/// <remarks>
		/// This never throws an exception. If an error occurs while parsing the Error property will be set with an error message.
		/// </remarks>
		/// <param name="stream">The stream of json to parse.</param>
		/// <returns>An object representing the json.</returns>
		public VersionedDeserializedRemoteSettings TryParseVersionedStream(Stream stream)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Invalid comparison between Unknown and I4
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Invalid comparison between Unknown and I4
			try
			{
				using (StreamReader streamReader = new StreamReader(stream))
				{
					JsonTextReader val = (JsonTextReader)(object)new JsonTextReader((TextReader)streamReader);
					try
					{
						JObject obj = JObject.Load((JsonReader)(object)val);
						Queue<PropertyEntry> queue = new Queue<PropertyEntry>();
						string fileVersion;
						string changesetId;
						using (IEnumerator<JProperty> enumerator = obj.Properties().GetEnumerator())
						{
							if (!enumerator.MoveNext() || enumerator.Current.Name != "FileVersion" || (int)enumerator.Current.Value.Type != 8)
							{
								return new VersionedDeserializedRemoteSettings(null, null, null, null, FileVersionNotFirstPropertyErrorMessage);
							}
							fileVersion = (string)enumerator.Current.Value;
							if (!enumerator.MoveNext() || enumerator.Current.Name != "ChangesetId" || (int)enumerator.Current.Value.Type != 8)
							{
								return new VersionedDeserializedRemoteSettings(null, null, null, null, ChangesetIdNotSecondPropertyErrorMessage);
							}
							changesetId = (string)enumerator.Current.Value;
							while (enumerator.MoveNext())
							{
								queue.Enqueue(new PropertyEntry(string.Empty, enumerator.Current));
							}
						}
						return new VersionedDeserializedRemoteSettings(ParseInternal(queue), fileVersion, changesetId);
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
				}
			}
			catch (JsonReaderException)
			{
				return new VersionedDeserializedRemoteSettings(null, null, null, null, InvalidJsonErrorMessage);
			}
			catch (Exception arg)
			{
				return new VersionedDeserializedRemoteSettings(null, null, null, null, string.Format(UnhandledExceptionErrorMessageFormat, arg));
			}
		}

		public DeserializedRemoteSettings TryParseFromJObject(JObject json, string globalScope = null)
		{
			Queue<PropertyEntry> queue = new Queue<PropertyEntry>();
			using (IEnumerator<JProperty> enumerator = json.Properties().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					queue.Enqueue(new PropertyEntry(string.Empty, enumerator.Current));
				}
			}
			return ParseInternal(queue, globalScope);
		}

		private DeserializedRemoteSettings ParseInternal(Queue<PropertyEntry> q, string globalScope = null)
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Invalid comparison between Unknown and I4
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Invalid comparison between Unknown and I4
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Invalid comparison between Unknown and I4
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Expected I4, but got Unknown
			//IL_026a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			PropertyEntry scopesPropertyEntry = q.Where((PropertyEntry x) => x.JProperty.Name == "Scopes").FirstOrDefault();
			List<Scope> list = new List<Scope>();
			if (!scopesPropertyEntry.Equals(default(PropertyEntry)))
			{
				if ((int)scopesPropertyEntry.JProperty.Value.Type != 1)
				{
					return new DeserializedRemoteSettings(null, null, ScopesWasNotObjectErrorMessage);
				}
				foreach (JProperty item in ((JObject)scopesPropertyEntry.JProperty.Value).Properties())
				{
					if ((int)item.Value.Type != 8)
					{
						return new DeserializedRemoteSettings(null, null, ScopeWasNotStringErrorMessage);
					}
					list.Add(new Scope
					{
						Name = item.Name,
						ScopeString = (string)item.Value
					});
				}
				q = new Queue<PropertyEntry>(q.Where((PropertyEntry x) => !x.Equals(scopesPropertyEntry)));
			}
			List<RemoteSetting> list2 = new List<RemoteSetting>();
			while (q.Count > 0)
			{
				PropertyEntry propertyEntry = q.Dequeue();
				JTokenType type = propertyEntry.JProperty.Value.Type;
				if ((int)type != 1)
				{
					switch (type - 6)
					{
					case (JTokenType)2:
						list2.Add(ProcessRemoteSetting(propertyEntry.Path, propertyEntry.JProperty.Name, (string)propertyEntry.JProperty.Value, globalScope));
						break;
					case (JTokenType)0:
						list2.Add(ProcessRemoteSetting(propertyEntry.Path, propertyEntry.JProperty.Name, (int)propertyEntry.JProperty.Value, globalScope));
						break;
					case (JTokenType)3:
						list2.Add(ProcessRemoteSetting(propertyEntry.Path, propertyEntry.JProperty.Name, (bool)propertyEntry.JProperty.Value, globalScope));
						break;
					default:
						return new DeserializedRemoteSettings(null, null, string.Format(TypeNotSupportedErrorMessageFormat, propertyEntry.JProperty.Value.Type));
					}
				}
				else
				{
					string path = (propertyEntry.Path == string.Empty) ? propertyEntry.JProperty.Name : Path.Combine(propertyEntry.Path, propertyEntry.JProperty.Name);
					foreach (JProperty item2 in ((JObject)propertyEntry.JProperty.Value).Properties())
					{
						q.Enqueue(new PropertyEntry(path, item2));
					}
				}
			}
			DeserializedRemoteSettings deserializedRemoteSettings = new DeserializedRemoteSettings(new ReadOnlyCollection<Scope>(list), new ReadOnlyCollection<RemoteSetting>(list2));
			try
			{
				remoteSettingsValidator.ValidateDeserialized(deserializedRemoteSettings);
				return deserializedRemoteSettings;
			}
			catch (RemoteSettingsValidationException ex)
			{
				return new DeserializedRemoteSettings(null, null, ex.Message);
			}
		}

		private static RemoteSetting ProcessRemoteSetting(string propertyPath, string propertyName, object value, string globalScope)
		{
			string name = propertyName;
			string scopeString = globalScope;
			int num = propertyName.IndexOf(':');
			if (num != -1)
			{
				name = propertyName.Substring(0, num);
				string text = propertyName.Substring(num + 1);
				scopeString = (string.IsNullOrEmpty(globalScope) ? text : (globalScope + " && " + text));
			}
			return new RemoteSetting(propertyPath, name, value, scopeString);
		}
	}
}

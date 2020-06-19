using Coding4Fun.VisualStudio.Telemetry.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal sealed class MemoryKeyValueStorage : ICollectionKeyValueStorage
	{
		private class KeyValueCollection
		{
			public HashSet<string> SubCollections
			{
				get;
				set;
			}

			public Dictionary<string, object> Properties
			{
				get;
				set;
			}

			public KeyValueCollection()
			{
				SubCollections = new HashSet<string>();
				Properties = new Dictionary<string, object>();
			}
		}

		private Dictionary<string, KeyValueCollection> collections = new Dictionary<string, KeyValueCollection>();

		public bool CollectionExists(string collectionPath)
		{
			collectionPath = collectionPath.NormalizePath();
			return collections.ContainsKey(collectionPath);
		}

		public bool PropertyExists(string collectionPath, string key)
		{
			collectionPath = collectionPath.NormalizePath();
			if (collections.TryGetValue(collectionPath, out KeyValueCollection value) && value.Properties.TryGetValue(key, out object _))
			{
				return true;
			}
			return false;
		}

		public bool DeleteCollection(string collectionPath)
		{
			throw new InvalidOperationException("MemoryKeyValueStorage does not support deleting collections");
		}

		public bool DeleteProperty(string collectionPath, string propertyName)
		{
			throw new InvalidOperationException("MemoryKeyValueStorage does not support deleting properties");
		}

		public IEnumerable<string> GetPropertyNames(string collectionPath)
		{
			collectionPath = collectionPath.NormalizePath();
			if (collections.TryGetValue(collectionPath, out KeyValueCollection value))
			{
				return value.Properties.Keys.ToList();
			}
			return Enumerable.Empty<string>();
		}

		public IEnumerable<string> GetSubCollectionNames(string collectionPath)
		{
			collectionPath = collectionPath.NormalizePath();
			if (collections.TryGetValue(collectionPath, out KeyValueCollection value))
			{
				return value.SubCollections.ToList();
			}
			return Enumerable.Empty<string>();
		}

		public T GetValue<T>(string collectionPath, string key, T defaultValue)
		{
			TryGetValueInternal(collectionPath, key, defaultValue, out T value);
			return value;
		}

		public bool TryGetValue<T>(string collectionPath, string key, out T value)
		{
			return TryGetValueInternal(collectionPath, key, default(T), out value);
		}

		public bool TryGetValueKind(string collectionPath, string key, out ValueKind kind)
		{
			collectionPath = collectionPath.NormalizePath();
			kind = ValueKind.Unknown;
			if (collections.TryGetValue(collectionPath, out KeyValueCollection value) && value.Properties.TryGetValue(key, out object value2))
			{
				Type type = value2.GetType();
				if (type == typeof(double) || type == typeof(ulong) || type == typeof(long))
				{
					kind = ValueKind.QWord;
				}
				else if (type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(float) || type == typeof(bool))
				{
					kind = ValueKind.DWord;
				}
				else if (type == typeof(string))
				{
					kind = ValueKind.String;
				}
				else
				{
					kind = ValueKind.Unknown;
				}
				return true;
			}
			return false;
		}

		public void SetValue<T>(string collectionPath, string key, T value)
		{
			collectionPath = collectionPath.NormalizePath();
			if (!collections.TryGetValue(collectionPath, out KeyValueCollection value2))
			{
				value2 = new KeyValueCollection();
				collections[collectionPath] = value2;
			}
			AddToParentCollections(collectionPath);
			value2.Properties[key] = value;
		}

		private void AddToParentCollections(string collectionPath)
		{
			if (collectionPath != string.Empty)
			{
				int num = collectionPath.LastIndexOf('\\');
				string text;
				string item;
				if (num != -1)
				{
					text = collectionPath.Substring(0, num);
					item = collectionPath.Substring(num + 1);
				}
				else
				{
					text = string.Empty;
					item = collectionPath;
				}
				if (!collections.TryGetValue(text, out KeyValueCollection value))
				{
					value = new KeyValueCollection();
					collections[text] = value;
				}
				value.SubCollections.Add(item);
				AddToParentCollections(text);
			}
		}

		private bool TryGetValueInternal<T>(string collectionPath, string key, T defaultValue, out T value)
		{
			collectionPath = collectionPath.NormalizePath();
			if (collections.TryGetValue(collectionPath, out KeyValueCollection value2) && value2.Properties.TryGetValue(key, out object value3))
			{
				return value3.TryConvertToType(defaultValue, out value);
			}
			value = defaultValue;
			return false;
		}
	}
}

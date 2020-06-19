using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Coding4Fun.VisualStudio.Telemetry
{
    /// <summary>
    ///     Persistant property bag on disk for machine storage.
    ///     To limit reads and writes the store is read in to memory upon creation and any values chagned via set property
    ///     are only persisted to physical storage upon calling Persist().
    ///     This will need to handle shared access as multiple instancnes/processes may attempt concurrent read/write operations.
    ///     The Stratagy is to minimumize the reads and writes and avoid locking the file, allowing concurrent read/write operations.
    ///     A backup helps deal with any file corruption issues.
    ///     All applications should continue to function without this file as if it is currupt or missing it will just be recreated.
    ///
    ///     We are serializing the Dictionary to Json prior to persisting to disk to enable non .Net CLR applications to read the store.
    /// </summary>
    internal sealed class MachinePropertyBag : IPersistentPropertyBag
    {
        private readonly string backUpStorageLocation;

        private readonly string storageLocation;

        private Dictionary<string, object> store;

        private bool storeWasChanged;

        internal MachinePropertyBag(string storageLocation)
        {
            this.storageLocation = storageLocation;
            backUpStorageLocation = GetBackupStoreLocation(this.storageLocation);
            LoadStore();
        }

        internal static string GetBackupStoreLocation(string primaryLocation)
        {
            return primaryLocation + ".bak";
        }

        /// <summary>Persists any in-memory changes to the store to disk.</summary>
        public void Persist()
        {
            if (storeWasChanged)
            {
                storeWasChanged = false;
                Persist(storageLocation);
                Persist(backUpStorageLocation);
            }
        }

        /// <summary>
        ///     Removes all properties from the in-memory copy of the PropertyBag.
        /// </summary>
        public void Clear()
        {
            store.Clear();
            storeWasChanged = true;
        }

        /// <summary>
        ///     Gets readonly copy of all properties with undefined order from the in memory copy of the store. Use the property bag methods for making changes to the in memory PropertyBag.
        /// </summary>
        /// <returns>A cloned copy of all properties from the PropertyBag.</returns>
        public IEnumerable<KeyValuePair<string, object>> GetAllProperties()
        {
            return store.Select((KeyValuePair<string, object> kvp) => new KeyValuePair<string, object>(kvp.Key, kvp.Value));
        }

        /// <summary>
        ///     Gets a specific property value from the in-memory copy or the PropertyBag.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object GetProperty(string propertyName)
        {
            object value = null;
            store.TryGetValue(propertyName, out value);
            return value;
        }

        /// <summary>
        ///     Removes a specific property value from the in-memory copy of the PropertyBag.
        /// </summary>
        /// <param name="propertyName"></param>
        public void RemoveProperty(string propertyName)
        {
            storeWasChanged = (storeWasChanged || store.Remove(propertyName));
        }

        /// <summary>
        ///     Sets a new integer property value in the in-memory copy of the PropertyBag.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void SetProperty(string propertyName, int value)
        {
            SetPropertyInternal(propertyName, value);
        }

        /// <summary>
        ///     Sets a new string property value in the in-memory copy of the PropertyBag.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void SetProperty(string propertyName, string value)
        {
            SetPropertyInternal(propertyName, value);
        }

        /// <summary>
        ///     Sets a new object property value in the in-memory copy of the PropertyBag.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void SetProperty(string propertyName, double value)
        {
            SetPropertyInternal(propertyName, value);
        }

        private void Persist(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                new BinaryFormatter().Serialize(fileStream, JsonConvert.SerializeObject((object)store));
                if (fileStream.Position != fileStream.Length)
                {
                    fileStream.SetLength(fileStream.Position);
                }
            }
            try
            {
                var file = new FileInfo(path);
                var accessControl = FileSystemAclExtensions.GetAccessControl(file);
                accessControl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, AccessControlType.Allow));
                accessControl.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-1-0"), FileSystemRights.FullControl, AccessControlType.Allow));
                FileSystemAclExtensions.SetAccessControl(file, accessControl);
            }
            catch (Exception)
            {
            }
        }

        private void LoadStore()
        {
            if (ParseIn(storageLocation))
            {
                if (!File.Exists(backUpStorageLocation))
                {
                    try
                    {
                        Persist(backUpStorageLocation);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else if (ParseIn(backUpStorageLocation))
            {
                try
                {
                    Persist(storageLocation);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                storeWasChanged = true;
                store = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private bool ParseIn(string filepath)
        {
            //IL_003b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0040: Unknown result type (might be due to invalid IL or missing references)
            //IL_004c: Expected O, but got Unknown
            if (File.Exists(filepath))
            {
                try
                {
                    using (FileStream serializationStream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        store = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        string obj = (string)new BinaryFormatter().Deserialize(serializationStream);
                        Dictionary<string, object> dictionary = store;
                        JsonSerializerSettings val = new JsonSerializerSettings();
                        val.DateParseHandling = ((DateParseHandling)0);
                        JsonConvert.PopulateObject(obj, (object)dictionary, (JsonSerializerSettings)(object)val);
                        KeyValuePair<string, object>[] array = store.Where((KeyValuePair<string, object> kvp) => kvp.Value is long).ToArray();
                        for (int i = 0; i < array.Length; i++)
                        {
                            KeyValuePair<string, object> keyValuePair = array[i];
                            store[keyValuePair.Key] = (int)(long)keyValuePair.Value;
                        }
                        return true;
                    }
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        private void SetPropertyInternal(string propertyName, object value)
        {
            if (!store.TryGetValue(propertyName, out object value2) || !object.Equals(value2, value))
            {
                storeWasChanged = true;
                store[propertyName] = value;
            }
        }
    }
}

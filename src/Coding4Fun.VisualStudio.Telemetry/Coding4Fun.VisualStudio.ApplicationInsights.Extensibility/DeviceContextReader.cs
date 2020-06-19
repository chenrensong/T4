using System;
using System.Globalization;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
    /// <summary>
    /// The reader is platform specific and applies to .NET applications only.
    /// </summary>
    /// <summary>
    /// The reader is platform specific and will contain different implementations for reading specific data based on the platform its running on.
    /// </summary>
    internal class DeviceContextReader : IDeviceContextReader
	{
		/// <summary>
		/// The device identifier.
		/// </summary>
		private string deviceId;

		/// <summary>
		/// The operating system.
		/// </summary>
		private string operatingSystem;

		/// <summary>
		/// The device manufacturer.
		/// </summary>
		private string deviceManufacturer;

		/// <summary>
		/// The device name.
		/// </summary>
		private string deviceName;

		/// <summary>
		/// The network type.
		/// </summary>
		private int? networkType;

		/// <summary>
		/// The cached fallback context.
		/// </summary>
		private FallbackDeviceContext cachedContext;

		/// <summary>
		/// The file name used when storing persistent context.
		/// </summary>
		internal const string ContextPersistentStorageFileName = "ApplicationInsights.DeviceContext.xml";

		/// <summary>
		/// The singleton instance for our reader.
		/// </summary>
		private static IDeviceContextReader instance;

		/// <summary>
		/// The sync root used in synchronizing access to persistent storage.
		/// </summary>
		private readonly object syncRoot = new object();

		/// <summary>
		/// Gets the fallback device context.
		/// </summary>
		public virtual FallbackDeviceContext FallbackContext => ReadSerializedContext();

		/// <summary>
		/// Gets or sets the singleton instance for our application context reader.
		/// </summary>
		public static IDeviceContextReader Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}
				Interlocked.CompareExchange(ref instance, new DeviceContextReader(), null);
				instance.Initialize();
				return instance;
			}
			internal set
			{
				instance = value;
			}
		}

		/// <summary>
		/// Initializes the current instance with respect to the platform specific implementation.
		/// </summary>
		public virtual void Initialize()
		{
		}

		/// <summary>
		/// Gets the type of the device.
		/// </summary>
		/// <returns>The type for this device as a hard-coded string.</returns>
		public virtual string GetDeviceType()
		{
			return "PC";
		}

		/// <summary>
		/// Gets the device unique ID, or uses the fallback if none is available due to application configuration.
		/// </summary>
		/// <returns>
		/// The discovered device identifier.
		/// </returns>
		public virtual string GetDeviceUniqueId()
		{
			if (deviceId != null)
			{
				return deviceId;
			}
			string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
			string text = Dns.GetHostName();
			if (!text.EndsWith(domainName, StringComparison.OrdinalIgnoreCase))
			{
				text = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2]
				{
					text,
					domainName
				});
			}
			return deviceId = text;
		}

		/// <summary>
		/// Gets the operating system.
		/// </summary>
		/// <returns>The discovered operating system.</returns>
		public virtual Task<string> GetOperatingSystemAsync()
		{
			if (operatingSystem == null)
			{
				operatingSystem = string.Format(CultureInfo.InvariantCulture, "Windows NT {0}", new object[1]
				{
					Environment.OSVersion.Version.ToString(4)
				});
			}
			TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
			taskCompletionSource.SetResult(operatingSystem);
			return taskCompletionSource.Task;
		}

		/// <summary>
		/// Gets the device OEM.
		/// </summary>
		/// <returns>The discovered OEM.</returns>
		public virtual string GetOemName()
		{
			if (deviceManufacturer != null)
			{
				return deviceManufacturer;
			}
			return deviceManufacturer = RunWmiQuery("Win32_ComputerSystem", "Manufacturer", string.Empty);
		}

		/// <summary>
		/// Gets the device model.
		/// </summary>
		/// <returns>The discovered device model.</returns>
		public virtual string GetDeviceModel()
		{
			if (deviceName != null)
			{
				return deviceName;
			}
			return deviceName = RunWmiQuery("Win32_ComputerSystem", "Model", string.Empty);
		}

		/// <summary>
		/// Gets the network type.
		/// </summary>
		/// <returns>The discovered network type.</returns>
		public int GetNetworkType()
		{
			if (networkType.HasValue)
			{
				return networkType.Value;
			}
			if (NetworkInterface.GetIsNetworkAvailable())
			{
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				foreach (NetworkInterface networkInterface in allNetworkInterfaces)
				{
					if (networkInterface.OperationalStatus == OperationalStatus.Up)
					{
						networkType = (int)networkInterface.NetworkInterfaceType;
						return networkType.Value;
					}
				}
			}
			networkType = 0;
			return networkType.Value;
		}

		/// <summary>
		/// Gets the screen resolution.
		/// </summary>
		/// <returns>The discovered screen resolution.</returns>
		public Task<string> GetScreenResolutionAsync()
		{
			TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
			taskCompletionSource.SetResult(string.Empty);
			return taskCompletionSource.Task;
		}

		/// <summary>
		/// Reads the serialized context from persistent storage, or will create a new context if none exits.
		/// </summary>
		/// <returns>The fallback context we will be using.</returns>
		private FallbackDeviceContext ReadSerializedContext()
		{
			if (cachedContext != null)
			{
				return cachedContext;
			}
			lock (syncRoot)
			{
				if (cachedContext != null)
				{
					return cachedContext;
				}
				FallbackDeviceContext fallbackDeviceContext = Utils.ReadSerializedContext<FallbackDeviceContext>("ApplicationInsights.DeviceContext.xml");
				Thread.MemoryBarrier();
				cachedContext = fallbackDeviceContext;
			}
			return cachedContext;
		}

		/// <summary>
		/// Runs a single WMI query for a property.
		/// </summary>
		/// <param name="table">The table.</param>
		/// <param name="property">The property.</param>
		/// <param name="defaultValue">The default value of the property if WMI fails.</param>
		/// <returns>The value if found, Unknown otherwise.</returns>
		private string RunWmiQuery(string table, string property, string defaultValue)
		{
			try
			{
				using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1}", new object[2]
				{
					property,
					table
				})))
				{
					foreach (ManagementObject item in managementObjectSearcher.Get())
					{
						object obj = item[property];
						if (obj != null)
						{
							return obj.ToString();
						}
					}
					return defaultValue;
				}
			}
			catch (Exception)
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.DeviceContextReader" /> class.
		/// </summary>
		internal DeviceContextReader()
		{
		}

		/// <summary>
		/// Gets the host system locale.
		/// </summary>
		/// <returns>The discovered locale.</returns>
		public virtual string GetHostSystemLocale()
		{
			return CultureInfo.CurrentCulture.Name;
		}
	}
}

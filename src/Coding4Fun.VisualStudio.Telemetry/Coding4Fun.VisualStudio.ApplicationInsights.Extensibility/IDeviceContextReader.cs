using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// The device context reader interface used while reading device related information in a platform specific way.
	/// </summary>
	internal interface IDeviceContextReader
	{
		/// <summary>
		/// Gets the fallback device context.
		/// </summary>
		FallbackDeviceContext FallbackContext
		{
			get;
		}

		/// <summary>
		/// Initializes the current reader with respect to its environment.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Gets the type of the device.
		/// </summary>
		/// <returns>The type for this device as a hard-coded string.</returns>
		string GetDeviceType();

		/// <summary>
		/// Gets the device unique identifier.
		/// </summary>
		/// <returns>The discovered device identifier.</returns>
		string GetDeviceUniqueId();

		/// <summary>
		/// Gets the operating system version.
		/// </summary>
		/// <returns>The discovered operating system.</returns>
		Task<string> GetOperatingSystemAsync();

		/// <summary>
		/// Gets the device OEM.
		/// </summary>
		/// <returns>The discovered OEM.</returns>
		string GetOemName();

		/// <summary>
		/// Gets the device model.
		/// </summary>
		/// <returns>The discovered device model.</returns>
		string GetDeviceModel();

		/// <summary>
		/// Gets the network type.
		/// </summary>
		/// <returns>The discovered network type.</returns>
		int GetNetworkType();

		/// <summary>
		/// Gets the screen resolution.
		/// </summary>
		/// <returns>The discovered screen resolution.</returns>
		Task<string> GetScreenResolutionAsync();

		/// <summary>
		/// Gets the host system locale.
		/// </summary>
		/// <returns>The discovered locale.</returns>
		string GetHostSystemLocale();
	}
}

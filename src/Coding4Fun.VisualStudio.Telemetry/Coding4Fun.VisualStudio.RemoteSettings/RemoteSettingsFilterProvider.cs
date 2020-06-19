using System;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	///  RemoteSettingsFilterProvider provides filter`s values.
	/// </summary>
	public abstract class RemoteSettingsFilterProvider
	{
		/// <summary>
		/// MachineId
		/// </summary>
		/// <returns></returns>
		public virtual Guid GetMachineId()
		{
			return Guid.Empty;
		}

		/// <summary>
		/// UserId
		/// </summary>
		/// <returns></returns>
		public virtual Guid GetUserId()
		{
			return Guid.Empty;
		}

		/// <summary>
		/// VSid of the signed-in user (VS specific - empty Guid if not applicable).
		/// </summary>
		/// <returns></returns>
		public virtual async Task<string> GetVsIdAsync()
		{
			return await Task.FromResult(string.Empty);
		}

		/// <summary>
		/// Culture string of the application.
		/// </summary>
		/// <returns></returns>
		public virtual string GetCulture()
		{
			return string.Empty;
		}

		/// <summary>
		/// Branch of the application (VS specific - empty string if not applicable).
		/// </summary>
		/// <returns></returns>
		public virtual string GetBranchBuildFrom()
		{
			return string.Empty;
		}

		/// <summary>
		/// Name of the application which uses remote settings service.
		/// </summary>
		/// <returns></returns>
		public virtual string GetApplicationName()
		{
			return string.Empty;
		}

		/// <summary>
		/// Version of the application which uses remote settings service.
		/// </summary>
		/// <returns></returns>
		public virtual string GetApplicationVersion()
		{
			return string.Empty;
		}

		/// <summary>
		/// Sku of the application (VS specific - empty string if not applicable).
		/// </summary>
		/// <returns></returns>
		public virtual string GetVsSku()
		{
			return string.Empty;
		}

		/// <summary>
		/// Number of notifications that have been sent
		/// </summary>
		/// <returns></returns>
		public virtual int GetNotificationsCount()
		{
			return 0;
		}

		/// <summary>
		/// Guid of the AppId package (VS specific - empty string if not available).
		/// </summary>
		/// <returns></returns>
		public virtual Guid GetAppIdPackageGuid()
		{
			return Guid.Empty;
		}

		/// <summary>
		/// MacAddressHash
		/// </summary>
		/// <returns></returns>
		public virtual string GetMacAddressHash()
		{
			return string.Empty;
		}

		/// <summary>
		/// Installation channel id.
		/// </summary>
		/// <returns></returns>
		public virtual string GetChannelId()
		{
			return string.Empty;
		}

		/// <summary>
		/// Installation channel manifest id.
		/// </summary>
		/// <returns></returns>
		public virtual string GetChannelManifestId()
		{
			return string.Empty;
		}

		/// <summary>
		/// Installation manifest id.
		/// </summary>
		/// <returns></returns>
		public virtual string GetManifestId()
		{
			return string.Empty;
		}

		/// <summary>
		/// OS type. Currently only "Windows"
		/// </summary>
		/// <returns></returns>
		public virtual string GetOsType()
		{
			return string.Empty;
		}

		/// <summary>
		/// Current OS Version
		/// </summary>
		/// <returns></returns>
		public virtual string GetOsVersion()
		{
			return string.Empty;
		}

		/// <summary>
		/// Whether or not user is Coding4Fun internal
		/// </summary>
		/// <returns></returns>
		public virtual bool GetIsUserInternal()
		{
			return false;
		}
	}
}

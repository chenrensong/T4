using Coding4Fun.VisualStudio.ApplicationInsights.DataContracts;
using Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation
{
	/// <summary>
	/// Encapsulates information about a device where an application is running.
	/// </summary>
	public sealed class DeviceContext : IJsonSerializable
	{
		private readonly IDictionary<string, string> tags;

		/// <summary>
		/// Gets or sets the type for the current device.
		/// </summary>
		public string Type
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceType);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.DeviceType, value);
			}
		}

		/// <summary>
		/// Gets or sets a device unique ID.
		/// </summary>
		public string Id
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceId);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.DeviceId, value);
			}
		}

		/// <summary>
		/// Gets or sets the operating system name.
		/// </summary>
		public string OperatingSystem
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceOSVersion);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.DeviceOSVersion, value);
			}
		}

		/// <summary>
		/// Gets or sets the device OEM for the current device.
		/// </summary>
		public string OemName
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceOEMName);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.DeviceOEMName, value);
			}
		}

		/// <summary>
		/// Gets or sets the device model for the current device.
		/// </summary>
		public string Model
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceModel);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.DeviceModel, value);
			}
		}

		/// <summary>
		/// Gets or sets the <a href="http://www.iana.org/assignments/ianaiftype-mib/ianaiftype-mib">IANA interface type</a>
		/// for the internet connected network adapter.
		/// </summary>
		public string NetworkType
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceNetwork);
			}
			set
			{
				tags.SetTagValueOrRemove(ContextTagKeys.Keys.DeviceNetwork, value);
			}
		}

		/// <summary>
		/// Gets or sets the current application screen resolution.
		/// </summary>
		public string ScreenResolution
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceScreenResolution);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.DeviceScreenResolution, value);
			}
		}

		/// <summary>
		/// Gets or sets the current display language of the operating system.
		/// </summary>
		public string Language
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceLanguage);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.DeviceLanguage, value);
			}
		}

		/// <summary>
		/// Gets or sets the role name.
		/// </summary>
		public string RoleName
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceRoleName);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.DeviceRoleName, value);
			}
		}

		/// <summary>
		/// Gets or sets the role instance.
		/// </summary>
		public string RoleInstance
		{
			get
			{
				return tags.GetTagValueOrNull(ContextTagKeys.Keys.DeviceRoleInstance);
			}
			set
			{
				tags.SetStringValueOrRemove(ContextTagKeys.Keys.DeviceRoleInstance, value);
			}
		}

		internal DeviceContext(IDictionary<string, string> tags)
		{
			this.tags = tags;
		}

		void IJsonSerializable.Serialize(IJsonWriter writer)
		{
			writer.WriteStartObject();
			writer.WriteProperty("type", Type);
			writer.WriteProperty("id", Utils.PopulateRequiredStringValue(Id, "id", typeof(DeviceContext).FullName));
			writer.WriteProperty("osVersion", OperatingSystem);
			writer.WriteProperty("oemName", OemName);
			writer.WriteProperty("model", Model);
			writer.WriteProperty("network", NetworkType);
			writer.WriteProperty("resolution", ScreenResolution);
			writer.WriteProperty("locale", Language);
			writer.WriteProperty("roleName", RoleName);
			writer.WriteProperty("roleInstance", RoleInstance);
			writer.WriteEndObject();
		}
	}
}

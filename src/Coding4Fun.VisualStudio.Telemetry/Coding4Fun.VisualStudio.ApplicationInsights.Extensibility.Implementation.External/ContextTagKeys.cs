using System.CodeDom.Compiler;
using System.Threading;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.External
{
	/// <summary>
	/// Holds the static singleton instance of ContextTagKeys.
	/// </summary>
	[GeneratedCode("gbc", "3.02")]
	internal class ContextTagKeys
	{
		private static ContextTagKeys keys;

		internal static ContextTagKeys Keys => LazyInitializer.EnsureInitialized(ref keys);

		public string ApplicationVersion
		{
			get;
			set;
		}

		public string ApplicationBuild
		{
			get;
			set;
		}

		public string DeviceId
		{
			get;
			set;
		}

		public string DeviceIp
		{
			get;
			set;
		}

		public string DeviceLanguage
		{
			get;
			set;
		}

		public string DeviceLocale
		{
			get;
			set;
		}

		public string DeviceModel
		{
			get;
			set;
		}

		public string DeviceNetwork
		{
			get;
			set;
		}

		public string DeviceOEMName
		{
			get;
			set;
		}

		public string DeviceOS
		{
			get;
			set;
		}

		public string DeviceOSVersion
		{
			get;
			set;
		}

		public string DeviceRoleInstance
		{
			get;
			set;
		}

		public string DeviceRoleName
		{
			get;
			set;
		}

		public string DeviceScreenResolution
		{
			get;
			set;
		}

		public string DeviceType
		{
			get;
			set;
		}

		public string DeviceMachineName
		{
			get;
			set;
		}

		public string LocationIp
		{
			get;
			set;
		}

		public string OperationId
		{
			get;
			set;
		}

		public string OperationName
		{
			get;
			set;
		}

		public string OperationParentId
		{
			get;
			set;
		}

		public string OperationRootId
		{
			get;
			set;
		}

		public string OperationSyntheticSource
		{
			get;
			set;
		}

		public string OperationIsSynthetic
		{
			get;
			set;
		}

		public string SessionId
		{
			get;
			set;
		}

		public string SessionIsFirst
		{
			get;
			set;
		}

		public string SessionIsNew
		{
			get;
			set;
		}

		public string UserAccountAcquisitionDate
		{
			get;
			set;
		}

		public string UserAccountId
		{
			get;
			set;
		}

		public string UserAgent
		{
			get;
			set;
		}

		public string UserId
		{
			get;
			set;
		}

		public string UserStoreRegion
		{
			get;
			set;
		}

		public string SampleRate
		{
			get;
			set;
		}

		public string InternalSdkVersion
		{
			get;
			set;
		}

		public string InternalAgentVersion
		{
			get;
			set;
		}

		public ContextTagKeys()
			: this("AI.ContextTagKeys", "ContextTagKeys")
		{
		}

		protected ContextTagKeys(string fullName, string name)
		{
			ApplicationVersion = "ai.application.ver";
			ApplicationBuild = "ai.application.build";
			DeviceId = "ai.device.id";
			DeviceIp = "ai.device.ip";
			DeviceLanguage = "ai.device.language";
			DeviceLocale = "ai.device.locale";
			DeviceModel = "ai.device.model";
			DeviceNetwork = "ai.device.network";
			DeviceOEMName = "ai.device.oemName";
			DeviceOS = "ai.device.os";
			DeviceOSVersion = "ai.device.osVersion";
			DeviceRoleInstance = "ai.device.roleInstance";
			DeviceRoleName = "ai.device.roleName";
			DeviceScreenResolution = "ai.device.screenResolution";
			DeviceType = "ai.device.type";
			DeviceMachineName = "ai.device.machineName";
			LocationIp = "ai.location.ip";
			OperationId = "ai.operation.id";
			OperationName = "ai.operation.name";
			OperationParentId = "ai.operation.parentId";
			OperationRootId = "ai.operation.rootId";
			OperationSyntheticSource = "ai.operation.syntheticSource";
			OperationIsSynthetic = "ai.operation.isSynthetic";
			SessionId = "ai.session.id";
			SessionIsFirst = "ai.session.isFirst";
			SessionIsNew = "ai.session.isNew";
			UserAccountAcquisitionDate = "ai.user.accountAcquisitionDate";
			UserAccountId = "ai.user.accountId";
			UserAgent = "ai.user.userAgent";
			UserId = "ai.user.id";
			UserStoreRegion = "ai.user.storeRegion";
			SampleRate = "ai.sample.sampleRate";
			InternalSdkVersion = "ai.internal.sdkVersion";
			InternalAgentVersion = "ai.internal.agentVersion";
		}
	}
}

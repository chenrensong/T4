using Newtonsoft.Json;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class ActionRequestParameters
	{
		[JsonProperty(PropertyName = "v0")]
		public string MachineId
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v1")]
		public string UserId
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v2")]
		public string VsoId
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v3")]
		public string Culture
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v4")]
		public string Version
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v5")]
		public string VsSku
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v6")]
		public int NotificationsCount
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v7")]
		public string AppIdPackage
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v8")]
		public string MacAddressHash
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v9")]
		public string ChannelId
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v10")]
		public string ChannelManifestId
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v11")]
		public string ManifestId
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v12")]
		public string OsType
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v13")]
		public string OsVersion
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v14")]
		public string ExeName
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v15")]
		public int IsInternal
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v16")]
		public IEnumerable<string> CachedRuleIds
		{
			get;
			set;
		}

		[JsonProperty(PropertyName = "v17")]
		public string SessionId
		{
			get;
			set;
		}
	}
}

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// Represents a single setting.
	/// </summary>
	internal sealed class RemoteSetting
	{
		public const char Separator = ':';

		/// <summary>
		/// Gets the path of the setting.
		/// </summary>
		public string Path
		{
			get;
		}

		/// <summary>
		/// Gets the name of the setting.
		/// </summary>
		public string Name
		{
			get;
		}

		/// <summary>
		/// Gets or sets the ScopeString associated with this setting, or Null.
		/// </summary>
		public string ScopeString
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the value of the setting.
		/// </summary>
		public object Value
		{
			get;
		}

		public string Origin
		{
			get;
			set;
		}

		public bool HasScope => ScopeString != null;

		public RemoteSetting(string path, string name, object value, string scopeString)
		{
			Path = path;
			Name = name;
			Value = value;
			ScopeString = scopeString;
			Origin = string.Empty;
		}

		public override string ToString()
		{
			string text = HasScope ? (":" + ScopeString) : string.Empty;
			return $"{Origin}: {Path} {Name}{text} {Value}";
		}
	}
}

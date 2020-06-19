namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class LoggingContext<T>
	{
		public string Context
		{
			get;
			set;
		}

		public T Value
		{
			get;
			set;
		}

		public LoggingContext(string context, T value)
		{
			Context = context;
			Value = value;
		}
	}
}

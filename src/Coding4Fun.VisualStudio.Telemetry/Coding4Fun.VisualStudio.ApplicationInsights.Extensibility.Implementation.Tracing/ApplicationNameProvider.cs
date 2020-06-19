using System;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing
{
	internal sealed class ApplicationNameProvider
	{
		public string Name
		{
			get;
			private set;
		}

		public ApplicationNameProvider()
		{
			Name = GetApplicationName();
		}

		private string GetApplicationName()
		{
			try
			{
				return AppDomain.CurrentDomain.FriendlyName;
			}
			catch (Exception ex)
			{
				return "Undefined " + ex.Message;
			}
		}
	}
}

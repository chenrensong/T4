namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Platform
{
	/// <summary>
	/// Provides access to the <see cref="P:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Platform.PlatformSingleton.Current" /> platform.
	/// </summary>
	internal static class PlatformSingleton
	{
		private static IPlatform current;

		/// <summary>
		/// Gets or sets the current <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.IPlatform" /> implementation.
		/// </summary>
		public static IPlatform Current
		{
			get
			{
				return current ?? (current = new PlatformImplementation());
			}
			set
			{
				current = value;
			}
		}
	}
}

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility.Implementation.Tracing
{
	internal static class EventSourceKeywords
	{
		public const long UserActionable = 1L;

		public const long Diagnostics = 2L;

		public const long VerboseFailure = 4L;

		public const long ErrorFailure = 8L;

		public const long ReservedUserKeywordBegin = 16L;
	}
}

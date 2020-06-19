namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// List of all priorities in order to easy maintain their order
	/// When you add new priority, please follow the ascending order.
	/// </summary>
	internal sealed class ActionPriorityConstants
	{
		public const int ManifestExcludePriority = 0;

		public const int CustomOptOutPriority = 1;

		public const int ManifestOptOutPriority = 2;

		public const int ManifestThrottlingPriority = 3;

		public const int ManifestPiiPriority = 4;

		public const int MetricPriority = 50;

		public const int SettingPriority = 60;

		public const int PiiPriority = 100;

		public const int EnforceAIRestrictionPriority = 200;

		public const int ComplexPropertyPriority = 250;

		public const int SuppressEmptyPostPropertyPriority = 300;

		public const int ThrottlingPriority = 1000;

		public const int ManifestRoutePriority = 10000;
	}
}

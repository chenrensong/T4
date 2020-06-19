namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class SettingAction : DataModelPropertyAction<TelemetrySettingProperty>
	{
		public SettingAction()
			: base(60, ".DataModelSetting", "HasSettings", "SettingProperties")
		{
		}
	}
}

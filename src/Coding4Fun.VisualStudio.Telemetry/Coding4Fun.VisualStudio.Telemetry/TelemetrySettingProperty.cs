namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This class represents a data model setting property.
	/// The property name will be updated with a suffix ".DataModelSetting" when the event is posted.
	/// A setting is something that user can customize the value to change how the app looks/feels/behaves.
	/// E.g., all settings in VS tools options dialog.
	/// Machine-level or environmental properties are NOT settings. They should posted as regular properties.
	/// E.g., CPU count, OS locale.
	/// </summary>
	public class TelemetrySettingProperty : TelemetryDataModelProperty
	{
		/// <summary>
		/// Creates the Setting Property Object.
		/// </summary>
		/// <param name="val"></param>
		public TelemetrySettingProperty(object val)
			: base(val)
		{
		}
	}
}

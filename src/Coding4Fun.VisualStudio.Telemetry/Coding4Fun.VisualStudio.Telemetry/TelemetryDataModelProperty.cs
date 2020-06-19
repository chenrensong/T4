namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This class represents a data model property.
	/// </summary>
	public abstract class TelemetryDataModelProperty
	{
		/// <summary>
		/// Gets value set for the property
		/// </summary>
		public object Value
		{
			get;
		}

		/// <summary>
		/// Creates the data model Property Object.
		/// </summary>
		/// <param name="val"></param>
		public TelemetryDataModelProperty(object val)
		{
			Value = val;
		}

		/// <summary>
		/// ToString to make debugging easier: show in debug watch window
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"{GetType().Name}({Value})";
		}
	}
}

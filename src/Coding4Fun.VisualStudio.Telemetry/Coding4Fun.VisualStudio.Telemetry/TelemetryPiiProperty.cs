using Coding4Fun.VisualStudio.Utilities.Internal;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This class represents a personally identifiable information property. During processing
	/// the raw value contained will be turned into a hashed value by a cryptology algorithm.
	/// </summary>
	public class TelemetryPiiProperty
	{
		/// <summary>
		/// Gets the string value contained to be hashed. We pre-ToString
		/// the value, since we need InvariantCulture to guarantee that values
		/// like double always produce the same results.
		/// </summary>
		public string StringValue
		{
			get;
		}

		/// <summary>
		/// Gets the raw value contained as the property value. We need the raw value
		/// since we want measurements for them, if available.
		/// </summary>
		public object RawValue
		{
			get;
		}

		/// <summary>
		/// Creates the Pii Object and converts to string. Throws if val is null.
		/// </summary>
		/// <param name="val"></param>
		public TelemetryPiiProperty(object val)
		{
			CodeContract.RequiresArgumentNotNull<object>(val, "val");
			RawValue = val;
			StringValue = TypeTools.ConvertToString(val);
		}

		/// <summary>
		/// ToString to make debugging easier: show in debug watch window
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"PII({RawValue})";
		}
	}
}

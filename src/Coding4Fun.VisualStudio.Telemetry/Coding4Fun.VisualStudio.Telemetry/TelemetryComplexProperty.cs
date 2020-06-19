using Coding4Fun.VisualStudio.Utilities.Internal;
using System.Diagnostics.CodeAnalysis;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// This class represents a complex value support, such as arrays, dictionaries. Processor will convert it to the JSON string.
	/// Also for such types we relax restrictions of the 1K for the property value.
	/// </summary>
	public class TelemetryComplexProperty
	{
		/// <summary>
		/// Gets the raw value contained as the property value. We need the raw value
		/// since we want measurements for them, if available.
		/// </summary>
		public object Value
		{
			get;
		}

		/// <summary>
		/// Creates the Complex Object. Throws if val is null.
		/// </summary>
		/// <param name="val"></param>
		public TelemetryComplexProperty(object val)
		{
			CodeContract.RequiresArgumentNotNull<object>(val, "val");
			Value = val;
		}

		/// <summary>
		/// ToString to make debugging easier: show in debug watch window
		/// </summary>
		/// <returns></returns>
		[ExcludeFromCodeCoverage]
		public override string ToString()
		{
			return $"Complex({Value})";
		}
	}
}

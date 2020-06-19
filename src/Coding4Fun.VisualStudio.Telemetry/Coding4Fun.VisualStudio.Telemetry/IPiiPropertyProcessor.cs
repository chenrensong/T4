using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal interface IPiiPropertyProcessor
	{
		/// <summary>
		/// Returns type of object we can process.
		/// </summary>
		/// <returns></returns>
		Type TypeOfPiiProperty();

		/// <summary>
		/// Convert object to the raw string representation.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		object ConvertToRawValue(object value);

		/// <summary>
		/// Convert object to the hashed string representation.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		string ConvertToHashedValue(object value);

		/// <summary>
		/// Check whether we can add raw property value.
		/// </summary>
		/// <param name="eventProcessorContext"></param>
		/// <returns></returns>
		bool CanAddRawValue(IEventProcessorContext eventProcessorContext);

		/// <summary>
		/// Generates raw property name based on the existing property name.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		string BuildRawPropertyName(string propertyName);
	}
}

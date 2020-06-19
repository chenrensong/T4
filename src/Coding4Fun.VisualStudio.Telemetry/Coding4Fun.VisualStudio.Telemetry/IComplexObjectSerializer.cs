using System;

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Interface is used to implement serializer to convert object to the string representation for the complex objects.
	/// </summary>
	internal interface IComplexObjectSerializer
	{
		/// <summary>
		/// Try to serialize object into the string
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		string Serialize(object obj);

		/// <summary>
		/// Set converter for the specific type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="converter"></param>
		void SetTypeConverter(Type type, Func<object, string> converter);

		/// <summary>
		/// Check whether converter for the specifc type was called.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		bool WasConverterUsedForType(Type type);
	}
}

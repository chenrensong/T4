using System;
using System.Collections.Generic;

namespace Coding4Fun.VisualStudio.ApplicationInsights.DataContracts
{
	/// <summary>
	/// Encapsulates logic for serializing objects to JSON.
	/// </summary>
	/// <seealso cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.IJsonSerializable" />.
	public interface IJsonWriter
	{
		/// <summary>
		/// Writes opening/left square bracket.
		/// </summary>
		void WriteStartArray();

		/// <summary>
		/// Writes opening/left curly brace.
		/// </summary>
		void WriteStartObject();

		/// <summary>
		/// Writes closing/right square bracket.
		/// </summary>
		void WriteEndArray();

		/// <summary>
		/// Writes closing/right curly brace.
		/// </summary>
		void WriteEndObject();

		/// <summary>
		/// Writes comma.
		/// </summary>
		void WriteComma();

		/// <summary>
		/// Writes a <see cref="T:System.String" /> property.
		/// </summary>
		void WriteProperty(string name, string value);

		/// <summary>
		/// Writes a <see cref="T:System.Boolean" /> property.
		/// </summary>
		void WriteProperty(string name, bool? value);

		/// <summary>
		/// Writes a <see cref="T:System.Int32" /> property.
		/// </summary>
		void WriteProperty(string name, int? value);

		/// <summary>
		/// Writes a <see cref="T:System.Double" /> property.
		/// </summary>
		void WriteProperty(string name, double? value);

		/// <summary>
		/// Writes a <see cref="T:System.TimeSpan" /> property.
		/// </summary>
		void WriteProperty(string name, TimeSpan? value);

		/// <summary>
		/// Writes a <see cref="T:System.DateTimeOffset" /> property.
		/// </summary>
		void WriteProperty(string name, DateTimeOffset? value);

		/// <summary>
		/// Writes a <see cref="T:System.Collections.Generic.IDictionary`2" /> property.
		/// </summary>
		void WriteProperty(string name, IDictionary<string, double> values);

		/// <summary>
		/// Writes a <see cref="T:System.Collections.Generic.IDictionary`2" /> property.
		/// </summary>
		void WriteProperty(string name, IDictionary<string, string> values);

		/// <summary>
		/// Writes an <see cref="T:Coding4Fun.VisualStudio.ApplicationInsights.DataContracts.IJsonSerializable" /> object property.
		/// </summary>
		void WriteProperty(string name, IJsonSerializable value);

		/// <summary>
		/// Writes a property name in double quotation marks, followed by a colon.
		/// </summary>
		void WritePropertyName(string name);

		/// <summary>
		/// Writes <see cref="T:System.Object" /> as raw value directly.
		/// </summary>
		void WriteRawValue(object value);
	}
}

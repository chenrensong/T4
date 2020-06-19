using System;
using System.Xml.Linq;

namespace Coding4Fun.VisualStudio.ApplicationInsights.Extensibility
{
	/// <summary>
	/// The fallback device context we will be using if we can't extract device information directly.
	/// </summary>
	internal class FallbackDeviceContext : IFallbackContext
	{
		/// <summary>
		/// Gets the device unique identifier.
		/// </summary>
		public string DeviceUniqueId
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes this instance with a set of new properties to serve as context.
		/// </summary>
		public void Initialize()
		{
			byte[] array = new byte[20];
			new Random().NextBytes(array);
			DeviceUniqueId = Convert.ToBase64String(array);
		}

		/// <summary>
		/// Serializes the current instance to the passed in root element.
		/// </summary>
		/// <param name="rootElement">The root element to serialize to.</param>
		public void Serialize(XElement rootElement)
		{
			rootElement.Add(new XElement(XName.Get("DeviceUniqueId"), DeviceUniqueId));
		}

		/// <summary>
		/// Deserializes the passed in root element to the current instance.
		/// </summary>
		/// <param name="rootElement">The root element to deserialize.</param>
		/// <returns>True if deserialization was successful, false otherwise.</returns>
		public bool Deserialize(XElement rootElement)
		{
			if (rootElement == null)
			{
				return false;
			}
			XElement xElement = rootElement.Element(XName.Get("DeviceUniqueId"));
			if (xElement == null)
			{
				return false;
			}
			DeviceUniqueId = xElement.Value;
			if (DeviceUniqueId.IsNullOrWhiteSpace())
			{
				return false;
			}
			return true;
		}
	}
}

namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// Create instance of the complex object serializer
	/// </summary>
	internal interface IComplexObjectSerializerFactory
	{
		IComplexObjectSerializer Instance();
	}
}

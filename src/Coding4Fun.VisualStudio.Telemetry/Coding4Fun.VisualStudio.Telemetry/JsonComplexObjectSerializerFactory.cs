namespace Coding4Fun.VisualStudio.Telemetry
{
	internal sealed class JsonComplexObjectSerializerFactory : IComplexObjectSerializerFactory
	{
		public IComplexObjectSerializer Instance()
		{
			return new JsonComplexObjectSerializer();
		}
	}
}

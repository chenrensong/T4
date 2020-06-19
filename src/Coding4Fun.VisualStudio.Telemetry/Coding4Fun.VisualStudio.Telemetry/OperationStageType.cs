namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// An enum to define operation stage type. Please don't change the enum name because it is part of client-server contract.
	/// </summary>
	public enum OperationStageType
	{
		/// <summary>
		/// Represent the full stage of an operation.
		/// Used when consumer doesn't need telemetry data for the operation duration.
		/// </summary>
		Atomic,
		/// <summary>
		/// Represent the start point of a long-running or async operation.
		/// </summary>
		Start,
		/// <summary>
		/// Represent the end point of a long-running or async operation.
		/// </summary>
		End
	}
}

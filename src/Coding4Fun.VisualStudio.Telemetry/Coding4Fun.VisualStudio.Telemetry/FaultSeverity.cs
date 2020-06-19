namespace Coding4Fun.VisualStudio.Telemetry
{
	/// <summary>
	/// An indicator of the severity of a given fault based on anticipated importance or impact.
	/// More severe faults will be promoted higher in reports, and less severe faults will be de-emphasized.
	/// </summary>
	public enum FaultSeverity
	{
		/// <summary>
		/// Uncategorized faults have no severity assigned by the developer. Developers should NOT use this severity in any new instrumentation.
		/// The majority of uncategorized faults are being assigned the uncategorized value by default in legacy code.
		/// Teams with high volumes of uncategorized fault data may be asked to make changes to add real severity to their faults.
		/// </summary>
		Uncategorized,
		/// <summary>
		/// Diagnostics faults represent faults which are likely informational in nature. The fault may have no clear tangible impact, it may
		/// be considered "by design" but still undesirable, or the fault only matters in relation to other faults. The fault information is
		/// nonetheless useful to investigate or root-cause an issue, or to inform future investments or changes to design, but the fault
		/// is not itself an indicator of an issue warranting attention.
		/// </summary>
		Diagnostic,
		/// <summary>
		/// General faults are the most common type of fault - the impact or significance of the fault may not be known during instrumentation.
		/// Further investigation may be required to understand the nature of the fault and, if possible, assign a more useful severity.
		/// </summary>
		General,
		/// <summary>
		/// Critical faults are faults which represent likely bugs or notable user impact. If this kind of fault is seen, there is a high
		/// likelihood that there is some kind of bug ultimately causing the issue.
		/// </summary>
		Critical
	}
}

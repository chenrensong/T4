namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// (Optional) interface that DirectiveProcessors can implement if they care about the value of the HostSpecific flag when they are generating code.
	/// <remarks>
	/// Will be called immediately after Initialize
	/// </remarks>
	/// </summary>
	public interface IRecognizeHostSpecific
	{
		/// <summary>
		/// Allow a directive processor to specify that it needs the run to be host-specific.
		/// </summary>
		/// <remarks>
		/// If any directive processor in the run sets this to be true then the engine will make the entire run host-specific.
		/// </remarks>
		bool RequiresProcessingRunIsHostSpecific
		{
			get;
		}

		/// <summary>
		/// Inform the directive processor whether the run is host-specific.
		/// </summary>
		/// <remarks>
		/// Will be called after RequiresProcessingRunIsHostSpecific has been run
		/// on all directive processors to inform the processor what the final host-specifc decision is
		/// </remarks>
		/// <param name="hostSpecific"></param>
		void SetProcessingRunIsHostSpecific(bool hostSpecific);
	}
}

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// A bool <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.ScopeValue" />
	/// </summary>
	public sealed class BoolScopeValue : ScopeValue
	{
		private bool value;

		/// <summary>
		/// Constructs a ScopeValue with the passed in bool value.
		/// </summary>
		/// <param name="value"></param>
		public BoolScopeValue(bool value)
		{
			this.value = value;
		}

		internal override AsyncScopeParser.AsyncOperand GetAsyncOperand()
		{
			return new AsyncScopeParser.BoolOperand(value);
		}

		internal override ScopeParser.Operand GetOperand()
		{
			return new ScopeParser.BoolOperand(value);
		}
	}
}

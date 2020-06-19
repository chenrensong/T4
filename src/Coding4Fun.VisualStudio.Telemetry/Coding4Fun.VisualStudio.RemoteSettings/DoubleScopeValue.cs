namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// A double <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.ScopeValue" />
	/// </summary>
	public sealed class DoubleScopeValue : ScopeValue
	{
		private double value;

		/// <summary>
		/// Constructs a ScopeValue with the passed in double value.
		/// </summary>
		/// <param name="value"></param>
		public DoubleScopeValue(double value)
		{
			this.value = value;
		}

		internal override AsyncScopeParser.AsyncOperand GetAsyncOperand()
		{
			return new AsyncScopeParser.DoubleOperand(value);
		}

		internal override ScopeParser.Operand GetOperand()
		{
			return new ScopeParser.DoubleOperand(value);
		}
	}
}

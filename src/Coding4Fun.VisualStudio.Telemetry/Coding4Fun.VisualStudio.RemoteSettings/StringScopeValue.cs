namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// A string <see cref="T:Coding4Fun.VisualStudio.RemoteSettings.ScopeValue" />
	/// </summary>
	public sealed class StringScopeValue : ScopeValue
	{
		private string value;

		/// <summary>
		/// Constructs a ScopeValue with the passed in string value.
		/// </summary>
		/// <param name="value"></param>
		public StringScopeValue(string value)
		{
			this.value = value;
		}

		internal override AsyncScopeParser.AsyncOperand GetAsyncOperand()
		{
			return new AsyncScopeParser.StringOperand(value);
		}

		internal override ScopeParser.Operand GetOperand()
		{
			return new ScopeParser.StringOperand(value);
		}
	}
}

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	/// <summary>
	/// A value that a Scope can have.
	/// </summary>
	public abstract class ScopeValue
	{
		internal abstract ScopeParser.Operand GetOperand();

		internal abstract AsyncScopeParser.AsyncOperand GetAsyncOperand();
	}
}

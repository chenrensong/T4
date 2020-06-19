using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace Coding4Fun.VisualStudio.TextTemplating.CodeDom
{
	/// <summary>
	/// Class to compare a pair of CodeAttributeDeclarations
	/// </summary>
	internal class CodeAttributeDeclarationComparer : IEqualityComparer<CodeAttributeDeclaration>
	{
		private static CodeAttributeDeclarationComparer instance = new CodeAttributeDeclarationComparer();

		public static CodeAttributeDeclarationComparer Instance => instance;

		public bool Equals(CodeAttributeDeclaration x, CodeAttributeDeclaration y)
		{
			if ((x == null && y != null) || (y == null && x != null))
			{
				return false;
			}
			if (x == null || y == null)
			{
				return true;
			}
			if (StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name) == 0 && StringComparer.OrdinalIgnoreCase.Compare(x.AttributeType.BaseType, y.AttributeType.BaseType) == 0 && x.Arguments.Count == y.Arguments.Count)
			{
				return x.Arguments.OfType<CodeAttributeArgument>().Except(y.Arguments.OfType<CodeAttributeArgument>(), CodeAttributeArgumentComparer.Instance).Count() == 0;
			}
			return false;
		}

		public int GetHashCode(CodeAttributeDeclaration obj)
		{
			return 1;
		}
	}
}

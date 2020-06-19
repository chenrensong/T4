using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Coding4Fun.VisualStudio.TextTemplating.CodeDom
{
	/// <summary>
	/// Class to compare a pair of CodeAttributeArguments using their string representations
	/// </summary>
	internal class CodeAttributeArgumentComparer : IEqualityComparer<CodeAttributeArgument>
	{
		private static CodeAttributeArgumentComparer instance = new CodeAttributeArgumentComparer();

		public static CodeAttributeArgumentComparer Instance => instance;

		public CodeDomProvider Provider
		{
			get;
			set;
		}

		private static CodeGeneratorOptions StandardOptions => new CodeGeneratorOptions
		{
			BlankLinesBetweenMembers = true,
			IndentString = "    ",
			VerbatimOrder = true,
			BracingStyle = "C"
		};

		public bool Equals(CodeAttributeArgument x, CodeAttributeArgument y)
		{
			if ((x == null && y != null) || (y == null && x != null))
			{
				return false;
			}
			if (x == null || y == null)
			{
				return true;
			}
			if (StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name) != 0)
			{
				return false;
			}
			StringBuilder stringBuilder = new StringBuilder();
			string x2;
			using (StringWriter writer = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
			{
				Provider.GenerateCodeFromExpression(x.Value, writer, StandardOptions);
				x2 = stringBuilder.ToString();
			}
			stringBuilder = new StringBuilder();
			string y2;
			using (StringWriter writer2 = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
			{
				Provider.GenerateCodeFromExpression(y.Value, writer2, StandardOptions);
				y2 = stringBuilder.ToString();
			}
			return StringComparer.OrdinalIgnoreCase.Compare(x2, y2) == 0;
		}

		public int GetHashCode(CodeAttributeArgument obj)
		{
			if (obj.Name != null)
			{
				return obj.Name.GetHashCode();
			}
			return 1;
		}
	}
}

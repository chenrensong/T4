using Coding4Fun.VisualStudio.TextTemplating.CodeDom;
using System;
using System.CodeDom;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Utility class to produce culture-oriented representation of an object as a string.
	/// </summary>
	public static class ToStringHelper
	{
		private static IFormatProvider formatProviderField = CultureInfo.InvariantCulture;

		/// <summary>
		/// Gets or sets format provider to be used by ToStringWithCulture method.
		/// </summary>
		public static IFormatProvider FormatProvider
		{
			[DebuggerStepThrough]
			get
			{
				return formatProviderField;
			}
			[DebuggerStepThrough]
			set
			{
				if (value != null)
				{
					formatProviderField = value;
				}
			}
		}

		/// <summary>
		/// This is called from the compile/run appdomain to convert objects within an expression block
		/// to a string
		/// </summary>
		/// <param name="objectToConvert">The object to convert to a string</param>
		/// <returns>The object converted to a string using the template's culture</returns>
		public static string ToStringWithCulture(object objectToConvert)
		{
			if (objectToConvert == null)
			{
				throw new ArgumentNullException("objectToConvert");
			}
			Type type = objectToConvert.GetType();
			MethodInfo method = type.GetMethod("ToString", new Type[1]
			{
				typeof(IFormatProvider)
			});
			if (method == null)
			{
				return objectToConvert.ToString();
			}
			return method.Invoke(objectToConvert, new object[1]
			{
				formatProviderField
			}) as string;
		}

		/// <summary>
		/// Get a set of CodeDOM members that provides the same functionality as this class as a property exposing a nested class.
		/// </summary>
		/// <param name="formatProvider">The default value of the format provider to use</param>
		internal static CodeTypeMemberCollection ProvideHelpers(CultureInfo formatProvider)
		{
			CodeTypeMemberCollection codeTypeMemberCollection = new CodeTypeMemberCollection();
			CodeTypeDeclaration codeTypeDeclaration = ProvideNestedClass(formatProvider, codeTypeMemberCollection);
			CodeTypeReference nestRef = new CodeTypeReference(codeTypeDeclaration.Name);
			CodeMemberField toStringHelperField = ProvideNestedClassField(codeTypeMemberCollection, nestRef);
			ProvideNestedClassProperty(codeTypeMemberCollection, nestRef, toStringHelperField);
			return codeTypeMemberCollection;
		}

		/// <summary>
		/// Provide a nested class to handle converting to string in a culture-sensitive manner
		/// </summary>
		/// <param name="formatProvider"></param>
		/// <param name="members"></param>
		/// <returns></returns>
		private static CodeTypeDeclaration ProvideNestedClass(CultureInfo formatProvider, CodeTypeMemberCollection members)
		{
			CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration("ToStringInstanceHelper")
			{
				IsClass = true
			};
			codeTypeDeclaration.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "ToString Helpers"));
			codeTypeDeclaration.Attributes = MemberAttributes.Public;
			codeTypeDeclaration.AddSummaryComment("Utility class to produce culture-oriented representation of an object as a string.");
			CodeMemberField codeMemberField = ProvideNestedFormatProviderField(codeTypeDeclaration, formatProvider);
			CodeFieldReferenceExpression codeFieldReferenceExpression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), codeMemberField.Name);
			ProvideNestedFormatProviderProperty(codeTypeDeclaration, codeFieldReferenceExpression);
			ProvideNestedToStringWithCultureMethod(codeTypeDeclaration, codeFieldReferenceExpression);
			members.Add(codeTypeDeclaration);
			return codeTypeDeclaration;
		}

		private static CodeMemberField ProvideNestedFormatProviderField(CodeTypeDeclaration nest, CultureInfo formatProvider)
		{
			CodeMemberField codeMemberField = new CodeMemberField(typeof(IFormatProvider), "formatProviderField ")
			{
				Attributes = MemberAttributes.Private
			};
			if (formatProvider == CultureInfo.InvariantCulture)
			{
				codeMemberField.InitExpression = typeof(CultureInfo).Expr().Prop("InvariantCulture");
			}
			else
			{
				codeMemberField.InitExpression = new CodeObjectCreateExpression(typeof(CultureInfo), formatProvider.Name.Prim());
			}
			nest.Members.Add(codeMemberField);
			return codeMemberField;
		}

		private static void ProvideNestedFormatProviderProperty(CodeTypeDeclaration nest, CodeFieldReferenceExpression formatProviderFieldRef)
		{
			CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
			codeMemberProperty.Name = "FormatProvider";
			codeMemberProperty.Attributes = (MemberAttributes)24578;
			codeMemberProperty.Type = new CodeTypeReference(typeof(IFormatProvider));
			CodeMemberProperty codeMemberProperty2 = codeMemberProperty;
			codeMemberProperty2.AddSummaryComment("Gets or sets format provider to be used by ToStringWithCulture method.");
			codeMemberProperty2.GetStatements.Add(new CodeMethodReturnStatement(formatProviderFieldRef));
			codeMemberProperty2.SetStatements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertySetValueReferenceExpression(), CodeBinaryOperatorType.IdentityInequality, CodeDomHelpers.nullEx), formatProviderFieldRef.Assign(new CodePropertySetValueReferenceExpression())));
			nest.Members.Add(codeMemberProperty2);
		}

		private static void ProvideNestedToStringWithCultureMethod(CodeTypeDeclaration nest, CodeFieldReferenceExpression formatProviderRef)
		{
			CodeParameterDeclarationExpression codeParameterDeclarationExpression = new CodeParameterDeclarationExpression(typeof(object), "objectToConvert");
			CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression("t");
			CodeVariableReferenceExpression codeVariableReferenceExpression2 = new CodeVariableReferenceExpression("method");
			CodeMemberMethod codeMemberMethod = CodeDomHelpers.CreateMethod(typeof(string), "ToStringWithCulture", "This is called from the compile/run appdomain to convert objects within an expression block to a string", (MemberAttributes)24578, CodeDomHelpers.CheckNullParameter(codeParameterDeclarationExpression.Name), new CodeVariableDeclarationStatement(typeof(Type), codeVariableReferenceExpression.VariableName, codeParameterDeclarationExpression.Ref().Call("GetType")), new CodeVariableDeclarationStatement(typeof(MethodInfo), codeVariableReferenceExpression2.VariableName, codeVariableReferenceExpression.Call("GetMethod", "ToString".Prim(), new CodeArrayCreateExpression(typeof(Type), new CodeExpression[1]
			{
				new CodeTypeOfExpression(typeof(IFormatProvider))
			}))), new CodeConditionStatement(new CodeBinaryOperatorExpression(codeVariableReferenceExpression2, CodeBinaryOperatorType.IdentityEquality, CodeDomHelpers.nullEx), new CodeStatement[1]
			{
				new CodeMethodReturnStatement(codeParameterDeclarationExpression.Ref().Call("ToString"))
			}, new CodeStatement[1]
			{
				new CodeMethodReturnStatement(new CodeCastExpression(typeof(string), codeVariableReferenceExpression2.Call("Invoke", codeParameterDeclarationExpression.Ref(), new CodeArrayCreateExpression(typeof(object), new CodeExpression[1]
				{
					formatProviderRef
				}))))
			}));
			codeMemberMethod.Parameters.Add(codeParameterDeclarationExpression);
			nest.Members.Add(codeMemberMethod);
		}

		private static CodeMemberField ProvideNestedClassField(CodeTypeMemberCollection members, CodeTypeReference nestRef)
		{
			CodeMemberField codeMemberField = new CodeMemberField(nestRef, "toStringHelperField")
			{
				Attributes = MemberAttributes.Private,
				InitExpression = new CodeObjectCreateExpression(nestRef)
			};
			members.Add(codeMemberField);
			return codeMemberField;
		}

		private static void ProvideNestedClassProperty(CodeTypeMemberCollection members, CodeTypeReference nestRef, CodeMemberField toStringHelperField)
		{
			CodeMemberProperty codeMemberProperty = new CodeMemberProperty
			{
				Type = nestRef,
				Name = "ToStringHelper",
				Attributes = (MemberAttributes)24578
			};
			codeMemberProperty.AddSummaryComment("Helper to produce culture-oriented representation of an object as a string");
			codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), toStringHelperField.Name)));
			codeMemberProperty.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "ToString Helpers"));
			members.Add(codeMemberProperty);
		}
	}
}

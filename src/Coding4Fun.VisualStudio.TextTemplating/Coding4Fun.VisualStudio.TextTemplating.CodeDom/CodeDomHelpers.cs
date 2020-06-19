using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;

namespace Coding4Fun.VisualStudio.TextTemplating.CodeDom
{
	/// <summary>
	/// Helper class with extension methods to aide in creating CodeDOM trees.
	/// </summary>
	/// <remarks>
	/// These extension methods allow a much terser and more expression-like syntax for constructing CodeDOM trees.
	/// </remarks>
	internal static class CodeDomHelpers
	{
		/// <summary>
		/// Simple static for the null expression
		/// </summary>
		internal static readonly CodePrimitiveExpression nullEx = new CodePrimitiveExpression(null);

		internal static CodeFieldReferenceExpression Ref(this CodeMemberField field)
		{
			return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), field.Name);
		}

		internal static CodePropertyReferenceExpression Ref(this CodeMemberProperty property)
		{
			return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), property.Name);
		}

		internal static CodeVariableReferenceExpression Ref(this CodeParameterDeclarationExpression parameter)
		{
			return new CodeVariableReferenceExpression(parameter.Name);
		}

		internal static CodeVariableReferenceExpression Ref(this CodeVariableDeclarationStatement declaration)
		{
			return new CodeVariableReferenceExpression(declaration.Name);
		}

		internal static CodeIndexerExpression Index(this CodeExpression expression, params CodeExpression[] indices)
		{
			return new CodeIndexerExpression(expression, indices);
		}

		internal static CodePropertyReferenceExpression Prop(this CodeExpression site, string property)
		{
			return new CodePropertyReferenceExpression(site, property);
		}

		internal static CodePrimitiveExpression Prim(this object primitive)
		{
			return new CodePrimitiveExpression(primitive);
		}

		internal static CodeTypeReferenceExpression Expr(this Type type)
		{
			CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression(type);
			codeTypeReferenceExpression.Type.Options = CodeTypeReferenceOptions.GlobalReference;
			return codeTypeReferenceExpression;
		}

		internal static CodeVariableDeclarationStatement Decl(this Type type, string name, CodeExpression initializer)
		{
			return new CodeVariableDeclarationStatement(type.Ref(), name, initializer);
		}

		internal static CodeTypeReference Ref(this Type type)
		{
			return new CodeTypeReference(type, CodeTypeReferenceOptions.GlobalReference);
		}

		internal static CodeObjectCreateExpression New(this Type type)
		{
			return new CodeObjectCreateExpression(type.Ref());
		}

		internal static CodeCastExpression Cast(this Type targetType, CodeExpression value)
		{
			return new CodeCastExpression(targetType.Ref(), value);
		}

		internal static CodeCastExpression Cast(this CodeTypeReference targetType, CodeExpression value)
		{
			return new CodeCastExpression(targetType, value);
		}

		internal static CodeObjectCreateExpression New(this Type type, params CodeExpression[] parameters)
		{
			return new CodeObjectCreateExpression(type.Ref(), parameters);
		}

		internal static CodeBinaryOperatorExpression VEquals(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.ValueEquality, rhs);
		}

		internal static CodeBinaryOperatorExpression VNEquals(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.ValueEquality, rhs), CodeBinaryOperatorType.ValueEquality, false.Prim());
		}

		internal static CodeBinaryOperatorExpression IEquals(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.IdentityEquality, rhs);
		}

		internal static CodeBinaryOperatorExpression INEquals(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.IdentityInequality, rhs);
		}

		internal static CodeBinaryOperatorExpression NotNull(this CodeExpression operand)
		{
			return operand.INEquals(nullEx);
		}

		internal static CodeBinaryOperatorExpression IsNull(this CodeExpression operand)
		{
			return operand.IEquals(nullEx);
		}

		internal static CodeBinaryOperatorExpression Add(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.Add, rhs);
		}

		internal static CodeBinaryOperatorExpression Subtract(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.Subtract, rhs);
		}

		internal static CodeBinaryOperatorExpression Gt(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.GreaterThan, rhs);
		}

		internal static CodeBinaryOperatorExpression Lt(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.LessThan, rhs);
		}

		internal static CodeBinaryOperatorExpression And(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.BooleanAnd, rhs);
		}

		internal static CodeBinaryOperatorExpression Or(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeBinaryOperatorExpression(lhs, CodeBinaryOperatorType.BooleanAnd, rhs);
		}

		internal static CodeBinaryOperatorExpression UnaryNot(this CodeExpression operand)
		{
			return new CodeBinaryOperatorExpression(operand, CodeBinaryOperatorType.ValueEquality, false.Prim());
		}

		internal static CodeAssignStatement Assign(this CodeExpression lhs, CodeExpression rhs)
		{
			return new CodeAssignStatement(lhs, rhs);
		}

		internal static CodeMethodInvokeExpression Call(this CodeExpression callSite, string methodName, params CodeExpression[] parameters)
		{
			return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(callSite, methodName), parameters);
		}

		internal static CodeExpressionStatement CallS(this CodeExpression callSite, string methodName, params CodeExpression[] parameters)
		{
			return new CodeExpressionStatement(callSite.Call(methodName, parameters));
		}

		internal static void AddSummaryComment(this CodeTypeMember member, string comment)
		{
			member.Comments.AddSummaryComment(comment);
		}

		internal static void AddSummaryComment(this CodeCommentStatementCollection comments, string comment)
		{
			comments.Add(new CodeCommentStatement(new CodeComment("<summary>", true)));
			comments.Add(new CodeCommentStatement(new CodeComment(comment, true)));
			comments.Add(new CodeCommentStatement(new CodeComment("</summary>", true)));
		}

		/// <summary>
		/// Call a static method as an expression
		/// </summary>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal static CodeMethodInvokeExpression Call(Type type, string methodName, params CodeExpression[] parameters)
		{
			return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(type.Expr(), methodName), parameters);
		}

		/// <summary>
		/// Call a static method as a statement
		/// </summary>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal static CodeExpressionStatement CallS(Type type, string methodName, params CodeExpression[] parameters)
		{
			return new CodeExpressionStatement(Call(type, methodName, parameters));
		}

		/// <summary>
		/// Call a method on an object as an expression
		/// </summary>
		/// <param name="callSite"></param>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal static CodeMethodInvokeExpression Call(string callSite, string methodName, params CodeExpression[] parameters)
		{
			return new CodeVariableReferenceExpression(callSite).Call(methodName, parameters);
		}

		/// <summary>
		/// Call a method on an object as a statement
		/// </summary>
		/// <param name="callSite"></param>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal static CodeExpressionStatement CallS(string callSite, string methodName, params CodeExpression[] parameters)
		{
			return new CodeExpressionStatement(Call(callSite, methodName, parameters));
		}

		/// <summary>
		/// Call a method on this class as an expression
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal static CodeMethodInvokeExpression Call(string methodName, params CodeExpression[] parameters)
		{
			return new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), methodName, parameters);
		}

		/// <summary>
		/// Call a method on this class as a statement
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal static CodeExpressionStatement CallS(string methodName, params CodeExpression[] parameters)
		{
			return new CodeExpressionStatement(Call(methodName, parameters));
		}

		/// <summary>
		/// Make a basic method
		/// </summary>
		/// <param name="returnType"></param>
		/// <param name="name"></param>
		/// <param name="summaryComment"></param>
		/// <param name="attributes"></param>
		/// <param name="statements"></param>
		/// <returns></returns>
		internal static CodeMemberMethod CreateMethod(Type returnType, string name, string summaryComment, MemberAttributes attributes, params CodeObject[] statements)
		{
			CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
			codeMemberMethod.ReturnType = ((returnType == null) ? null : returnType.Ref());
			codeMemberMethod.Name = name;
			codeMemberMethod.AddSummaryComment(summaryComment);
			codeMemberMethod.Attributes = attributes;
			foreach (CodeObject codeObject in statements)
			{
				CodeStatement codeStatement = codeObject as CodeStatement;
				CodeExpression codeExpression = codeObject as CodeExpression;
				if (codeStatement != null)
				{
					codeMemberMethod.Statements.Add(codeStatement);
				}
				if (codeExpression != null)
				{
					codeMemberMethod.Statements.Add(codeExpression);
				}
			}
			return codeMemberMethod;
		}

		/// <summary>
		/// Create the code for if (a==null) a = new Foo();
		/// </summary>
		/// <param name="variableRef"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static CodeConditionStatement IfVariableNullThenInstantiateType(CodeFieldReferenceExpression variableRef, Type type)
		{
			return new CodeConditionStatement(new CodeBinaryOperatorExpression(variableRef, CodeBinaryOperatorType.IdentityEquality, nullEx), new CodeAssignStatement(variableRef, type.New()));
		}

		internal static CodeConditionStatement CheckNullParameter(string name)
		{
			return new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(name), CodeBinaryOperatorType.IdentityEquality, nullEx), new CodeThrowExceptionStatement(typeof(ArgumentNullException).New(name.Prim())));
		}

		internal static CodeConditionStatement IfVariableNotNull(CodeVariableReferenceExpression variable, params CodeStatement[] trueStatements)
		{
			return new CodeConditionStatement(variable.INEquals(nullEx), trueStatements);
		}

		internal static CodeConditionStatement IfNotNull(this CodeVariableReferenceExpression variable, params CodeStatement[] trueStatements)
		{
			return IfVariableNotNull(variable, trueStatements);
		}

		/// <summary>
		/// Make a variable declaration for a PropertyInfo initialized with a GetProperty call on a target's type.
		/// <remarks>
		/// Creates code similar to: PropertyInfo name = target.GetType().GetProperty(propertyName);
		/// </remarks>
		/// </summary>
		internal static CodeVariableDeclarationStatement CreatePropertyInfoDeclaration(string name, CodeExpression target, string propertyName)
		{
			return new CodeVariableDeclarationStatement(typeof(PropertyInfo), name, target.Call("GetType").Call("GetProperty", propertyName.Prim()));
		}

		/// <summary>
		/// Make a variable declaration for an object initialized via a call to PropertyInfo.GetValue via a given instance
		/// <remarks>
		/// Creates code similar to: object name = propertyInfo.GetValue(instance, null|new object[] {parameters});
		/// </remarks>
		/// </summary>
		internal static CodeVariableDeclarationStatement CreatePropertyInfoValueDeclaration(string name, CodeExpression propertyInfo, CodeExpression instance, params CodeExpression[] indexParameters)
		{
			List<CodeExpression> list = new List<CodeExpression>
			{
				instance
			};
			if (indexParameters.Length == 0)
			{
				list.Add(nullEx);
			}
			else
			{
				list.Add(new CodeArrayCreateExpression(typeof(object), indexParameters));
			}
			return new CodeVariableDeclarationStatement(typeof(object), name, propertyInfo.Call("GetValue", list.ToArray()));
		}
	}
}

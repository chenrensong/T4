using Coding4Fun.VisualStudio.TextTemplating.CodeDom;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Base class for generated text transformations
	/// </summary>
	/// <remarks>
	/// Any class specified in an inherits directive must match this class in a duck-typing style.
	///
	/// Note that this class therefore specifies an implicit contract with the transformation object.
	/// The object doesn't have to derive from any specific type or interface, but it must have
	/// a) A void Initialize() method.
	/// b) A string TransformText() method
	/// c) An Errors property that's duck-compatible with CompilerErrorCollection
	/// d) A GeneratonEnvironment property that's duck-compatible with StringBuilder.
	/// e) A void Write(string) method 
	///
	/// Using any further features of T4 such as expression blocks will require the class to have further methods, such as ToStringHelper, but
	/// those will produce regular compiler errors at transform time that the base class author can address.
	///
	/// These few methods together form a subset of the TextTransformation default base class' API.
	///
	/// If you change this pseudo-contract to add more requirements, you should consider this a breaking change.
	/// It's OK, however, to change the contract to have fewer requirements.
	/// </remarks>
	public abstract class TextTransformation : IDisposable
	{
		private StringBuilder generationEnvironmentField;

		private CompilerErrorCollection errorsField;

		private List<int> indentLengthsField;

		private string currentIndentField = "";

		private bool endsWithNewline;

		private IDictionary<string, object> sessionField;

		private static CodeTypeMemberCollection baseClassMembers;

		/// <summary>
		/// The string builder that generation-time code is using to assemble generated output
		/// </summary>
		protected StringBuilder GenerationEnvironment
		{
			[DebuggerStepThrough]
			get
			{
				if (generationEnvironmentField == null)
				{
					generationEnvironmentField = new StringBuilder();
				}
				return generationEnvironmentField;
			}
			[DebuggerStepThrough]
			set
			{
				generationEnvironmentField = value;
			}
		}

		/// <summary>
		/// The error collection for the generation process
		/// </summary>
		public CompilerErrorCollection Errors
		{
			[DebuggerStepThrough]
			get
			{
				if (errorsField == null)
				{
					errorsField = new CompilerErrorCollection();
				}
				return errorsField;
			}
		}

		/// <summary>
		/// A list of the lengths of each indent that was added with PushIndent
		/// </summary>
		private List<int> indentLengths
		{
			get
			{
				if (indentLengthsField == null)
				{
					indentLengthsField = new List<int>();
				}
				return indentLengthsField;
			}
		}

		/// <summary>
		/// Gets the current indent we use when adding lines to the output
		/// </summary>
		public string CurrentIndent => currentIndentField;

		/// <summary>
		/// Current transformation session
		/// </summary>
		public virtual IDictionary<string, object> Session
		{
			get
			{
				return sessionField;
			}
			set
			{
				sessionField = value;
			}
		}

		/// <summary>
		/// Generate the output text of the transformation
		/// </summary>
		/// <returns></returns>
		public abstract string TransformText();

		/// <summary>
		/// Initialize the templating class
		/// </summary>
		/// <remarks>
		/// Derived classes are allowed to return errors from initialization 
		/// </remarks>
		public virtual void Initialize()
		{
		}

		/// <summary>
		/// Finaizlier.
		/// </summary>
		~TextTransformation()
		{
			Dispose(false);
		}

		/// <summary>
		/// Disposes the state of this object.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose implementation.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			generationEnvironmentField = null;
			errorsField = null;
		}

		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		/// <param name="textToAppend"></param>
		public void Write(string textToAppend)
		{
			if (string.IsNullOrEmpty(textToAppend))
			{
				return;
			}
			if (GenerationEnvironment.Length == 0 || endsWithNewline)
			{
				GenerationEnvironment.Append(currentIndentField);
				endsWithNewline = false;
			}
			if (textToAppend.EndsWith(Environment.NewLine, StringComparison.CurrentCulture))
			{
				endsWithNewline = true;
			}
			if (currentIndentField.Length == 0)
			{
				GenerationEnvironment.Append(textToAppend);
				return;
			}
			textToAppend = textToAppend.Replace(Environment.NewLine, Environment.NewLine + currentIndentField);
			if (endsWithNewline)
			{
				GenerationEnvironment.Append(textToAppend, 0, textToAppend.Length - currentIndentField.Length);
			}
			else
			{
				GenerationEnvironment.Append(textToAppend);
			}
		}

		/// <summary>
		/// Write text directly into the generated output
		/// </summary>
		/// <param name="textToAppend"></param>
		public void WriteLine(string textToAppend)
		{
			Write(textToAppend);
			GenerationEnvironment.AppendLine();
			endsWithNewline = true;
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void Write(string format, params object[] args)
		{
			Write(string.Format(CultureInfo.CurrentCulture, format, args));
		}

		/// <summary>
		/// Write formatted text directly into the generated output
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void WriteLine(string format, params object[] args)
		{
			WriteLine(string.Format(CultureInfo.CurrentCulture, format, args));
		}

		/// <summary>
		/// Raise an error
		/// </summary>
		/// <param name="message"></param>
		public void Error(string message)
		{
			CompilerError value = new CompilerError
			{
				ErrorText = message
			};
			Errors.Add(value);
		}

		/// <summary>
		/// Raise a warning
		/// </summary>
		/// <param name="message"></param>
		public void Warning(string message)
		{
			CompilerError value = new CompilerError
			{
				ErrorText = message,
				IsWarning = true
			};
			Errors.Add(value);
		}

		/// <summary>
		/// Increase the indent
		/// </summary>
		/// <param name="indent">indent string</param>
		public void PushIndent(string indent)
		{
			if (indent == null)
			{
				throw new ArgumentNullException("indent");
			}
			currentIndentField += indent;
			indentLengths.Add(indent.Length);
		}

		/// <summary>
		/// Remove the last indent that was added with PushIndent
		/// </summary>
		/// <returns>The removed indent string</returns>
		public string PopIndent()
		{
			string result = "";
			if (indentLengths.Count > 0)
			{
				int num = indentLengths[indentLengths.Count - 1];
				indentLengths.RemoveAt(indentLengths.Count - 1);
				if (num > 0)
				{
					result = currentIndentField.Substring(currentIndentField.Length - num);
					currentIndentField = currentIndentField.Remove(currentIndentField.Length - num);
				}
			}
			return result;
		}

		/// <summary>
		/// Remove any indentation
		/// </summary>
		public void ClearIndent()
		{
			indentLengths.Clear();
			currentIndentField = "";
		}

		internal static CodeTypeDeclaration ProvideBaseClass(string name)
		{
			CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(name);
			codeTypeDeclaration.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Base class"));
			codeTypeDeclaration.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "Base class"));
			codeTypeDeclaration.IsPartial = false;
			codeTypeDeclaration.IsClass = true;
			codeTypeDeclaration.AddSummaryComment("Base class for this transformation");
			if (baseClassMembers == null)
			{
				baseClassMembers = ProvideBaseClassMembers();
			}
			codeTypeDeclaration.Members.AddRange(baseClassMembers);
			return codeTypeDeclaration;
		}

		/// <summary>
		/// Get a set of CodeDOM members that match the real members on this class
		/// </summary>
		/// <remarks>
		/// If you add a member to this class, consider adding it to the list returned by this method.
		/// </remarks>
		private static CodeTypeMemberCollection ProvideBaseClassMembers()
		{
			CodeTypeMemberCollection codeTypeMemberCollection = new CodeTypeMemberCollection();
			CodeMemberField codeMemberField = new CodeMemberField(typeof(StringBuilder), "generationEnvironmentField");
			codeMemberField.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Fields"));
			codeMemberField.Type.Options = CodeTypeReferenceOptions.GlobalReference;
			codeTypeMemberCollection.Add(codeMemberField);
			CodeMemberField codeMemberField2 = new CodeMemberField(typeof(CompilerErrorCollection), "errorsField");
			codeMemberField2.Type.Options = CodeTypeReferenceOptions.GlobalReference;
			codeTypeMemberCollection.Add(codeMemberField2);
			CodeMemberField codeMemberField3 = new CodeMemberField(typeof(List<int>), "indentLengthsField");
			codeMemberField3.Type.Options = CodeTypeReferenceOptions.GlobalReference;
			codeTypeMemberCollection.Add(codeMemberField3);
			CodeMemberField codeMemberField4 = new CodeMemberField(typeof(string), "currentIndentField");
			codeMemberField4.Type.Options = CodeTypeReferenceOptions.GlobalReference;
			codeMemberField4.InitExpression = string.Empty.Prim();
			codeTypeMemberCollection.Add(codeMemberField4);
			CodeMemberField codeMemberField5 = new CodeMemberField(typeof(bool), "endsWithNewline");
			codeMemberField5.Type.Options = CodeTypeReferenceOptions.GlobalReference;
			codeTypeMemberCollection.Add(codeMemberField5);
			CodeMemberField codeMemberField6 = new CodeMemberField(typeof(IDictionary<string, object>), "sessionField");
			codeMemberField6.Type.Options = CodeTypeReferenceOptions.GlobalReference;
			codeMemberField6.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "Fields"));
			codeTypeMemberCollection.Add(codeMemberField6);
			CodeMemberProperty generationEnvironment = ProvideGenerationEnvironmentProperty(codeTypeMemberCollection, codeMemberField);
			CodeMemberProperty errors = ProvideErrorsProperty(codeTypeMemberCollection, codeMemberField2);
			CodeMemberProperty indentLengths = ProvideIndentLengthsProperty(codeTypeMemberCollection, codeMemberField3);
			ProvideCurrentIndentProperty(codeTypeMemberCollection, codeMemberField4);
			ProvideSessionProperty(codeTypeMemberCollection, codeMemberField6);
			CodeVariableReferenceExpression textToAppend = new CodeVariableReferenceExpression("textToAppend");
			CodeParameterDeclarationExpression argsParam = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object[])), "args");
			CodeVariableReferenceExpression error = new CodeVariableReferenceExpression("error");
			ProvideWriteMethod1(codeTypeMemberCollection, codeMemberField4, codeMemberField5, generationEnvironment, textToAppend);
			ProvideWriteLineMethod1(codeTypeMemberCollection, codeMemberField5, generationEnvironment, textToAppend);
			ProvideWriteMethod2(codeTypeMemberCollection, argsParam);
			ProvideWriteLineMethod2(codeTypeMemberCollection, argsParam);
			ProvideErrorMethod(codeTypeMemberCollection, errors, error);
			ProvideWarningMethod(codeTypeMemberCollection, errors, error);
			ProvidePushIndentMethod(codeTypeMemberCollection, codeMemberField4, indentLengths);
			ProvidePopIndentMethod(codeTypeMemberCollection, codeMemberField4, indentLengths);
			ProvideClearIndentMethod(codeTypeMemberCollection, codeMemberField4, indentLengths);
			return codeTypeMemberCollection;
		}

		private static void ProvideClearIndentMethod(CodeTypeMemberCollection members, CodeMemberField currentIndent, CodeMemberProperty indentLengths)
		{
			CodeMemberMethod codeMemberMethod = CodeDomHelpers.CreateMethod(null, "ClearIndent", "Remove any indentation", (MemberAttributes)24578, indentLengths.Ref().CallS("Clear"), currentIndent.Ref().Assign(string.Empty.Prim()));
			codeMemberMethod.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "Transform-time helpers"));
			members.Add(codeMemberMethod);
		}

		private static void ProvidePopIndentMethod(CodeTypeMemberCollection members, CodeMemberField currentIndent, CodeMemberProperty indentLengths)
		{
			CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression("indentLength");
			CodeVariableReferenceExpression codeVariableReferenceExpression2 = new CodeVariableReferenceExpression("returnValue");
			CodeMemberMethod value = CodeDomHelpers.CreateMethod(typeof(string), "PopIndent", "Remove the last indent that was added with PushIndent", (MemberAttributes)24578, new CodeVariableDeclarationStatement(typeof(string), codeVariableReferenceExpression2.VariableName, string.Empty.Prim()), new CodeConditionStatement(indentLengths.Ref().Prop("Count").Gt(0.Prim()), new CodeVariableDeclarationStatement(typeof(int), codeVariableReferenceExpression.VariableName, indentLengths.Ref().Index(indentLengths.Ref().Prop("Count").Subtract(1.Prim()))), indentLengths.Ref().CallS("RemoveAt", indentLengths.Ref().Prop("Count").Subtract(1.Prim())), new CodeConditionStatement(codeVariableReferenceExpression.Gt(0.Prim()), codeVariableReferenceExpression2.Assign(currentIndent.Ref().Call("Substring", currentIndent.Ref().Prop("Length").Subtract(codeVariableReferenceExpression))), currentIndent.Ref().Assign(currentIndent.Ref().Call("Remove", currentIndent.Ref().Prop("Length").Subtract(codeVariableReferenceExpression))))), new CodeMethodReturnStatement(codeVariableReferenceExpression2));
			members.Add(value);
		}

		private static void ProvidePushIndentMethod(CodeTypeMemberCollection members, CodeMemberField currentIndent, CodeMemberProperty indentLengths)
		{
			CodeVariableReferenceExpression codeVariableReferenceExpression = new CodeVariableReferenceExpression("indent");
			CodeMemberMethod codeMemberMethod = CodeDomHelpers.CreateMethod(null, "PushIndent", "Increase the indent", (MemberAttributes)24578, new CodeConditionStatement(codeVariableReferenceExpression.VEquals(CodeDomHelpers.nullEx), new CodeThrowExceptionStatement(typeof(ArgumentNullException).New("indent".Prim()))), currentIndent.Ref().Assign(currentIndent.Ref().Add(codeVariableReferenceExpression)), indentLengths.Ref().CallS("Add", codeVariableReferenceExpression.Prop("Length")));
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "indent"));
			members.Add(codeMemberMethod);
		}

		private static void ProvideWarningMethod(CodeTypeMemberCollection members, CodeMemberProperty Errors, CodeVariableReferenceExpression error)
		{
			CodeMemberMethod codeMemberMethod = CodeDomHelpers.CreateMethod(null, "Warning", "Raise a warning", (MemberAttributes)24578, new CodeVariableDeclarationStatement(typeof(CompilerError), "error", typeof(CompilerError).New()), error.Prop("ErrorText").Assign(new CodeVariableReferenceExpression("message")), error.Prop("IsWarning").Assign(true.Prim()), Errors.Ref().CallS("Add", error));
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "message"));
			members.Add(codeMemberMethod);
		}

		private static void ProvideErrorMethod(CodeTypeMemberCollection members, CodeMemberProperty Errors, CodeVariableReferenceExpression error)
		{
			CodeMemberMethod codeMemberMethod = CodeDomHelpers.CreateMethod(null, "Error", "Raise an error", (MemberAttributes)24578, new CodeVariableDeclarationStatement(typeof(CompilerError), "error", typeof(CompilerError).New()), error.Prop("ErrorText").Assign(new CodeVariableReferenceExpression("message")), Errors.Ref().CallS("Add", error));
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "message"));
			members.Add(codeMemberMethod);
		}

		private static void ProvideWriteLineMethod2(CodeTypeMemberCollection members, CodeParameterDeclarationExpression argsParam)
		{
			CodeMemberMethod codeMemberMethod = CodeDomHelpers.CreateMethod(null, "WriteLine", "Write formatted text directly into the generated output", (MemberAttributes)24578, CodeDomHelpers.Call("WriteLine", CodeDomHelpers.Call(typeof(string), "Format", typeof(CultureInfo).Expr().Prop("CurrentCulture"), new CodeVariableReferenceExpression("format"), new CodeVariableReferenceExpression("args"))));
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "format"));
			codeMemberMethod.Parameters.Add(argsParam);
			members.Add(codeMemberMethod);
		}

		private static void ProvideWriteMethod2(CodeTypeMemberCollection members, CodeParameterDeclarationExpression argsParam)
		{
			CodeMemberMethod codeMemberMethod = CodeDomHelpers.CreateMethod(null, "Write", "Write formatted text directly into the generated output", (MemberAttributes)24578, CodeDomHelpers.Call("Write", CodeDomHelpers.Call(typeof(string), "Format", typeof(CultureInfo).Expr().Prop("CurrentCulture"), new CodeVariableReferenceExpression("format"), new CodeVariableReferenceExpression("args"))));
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "format"));
			argsParam.CustomAttributes.Add(new CodeAttributeDeclaration("System.ParamArrayAttribute"));
			codeMemberMethod.Parameters.Add(argsParam);
			members.Add(codeMemberMethod);
		}

		private static void ProvideWriteLineMethod1(CodeTypeMemberCollection members, CodeMemberField endsWithNewline, CodeMemberProperty GenerationEnvironment, CodeVariableReferenceExpression textToAppend)
		{
			CodeMemberMethod codeMemberMethod = CodeDomHelpers.CreateMethod(null, "WriteLine", "Write text directly into the generated output", (MemberAttributes)24578, CodeDomHelpers.Call("Write", textToAppend), GenerationEnvironment.Ref().CallS("AppendLine"), endsWithNewline.Ref().Assign(true.Prim()));
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), textToAppend.VariableName));
			members.Add(codeMemberMethod);
		}

		private static void ProvideWriteMethod1(CodeTypeMemberCollection members, CodeMemberField currentIndent, CodeMemberField endsWithNewline, CodeMemberProperty GenerationEnvironment, CodeVariableReferenceExpression textToAppend)
		{
			CodeExpression codeExpression = typeof(Environment).Expr().Prop("NewLine");
			CodeMemberMethod codeMemberMethod = CodeDomHelpers.CreateMethod(null, "Write", "Write text directly into the generated output", (MemberAttributes)24578, new CodeConditionStatement(CodeDomHelpers.Call(typeof(string), "IsNullOrEmpty", textToAppend), new CodeMethodReturnStatement()), new CodeCommentStatement("If we're starting off, or if the previous text ended with a newline,"), new CodeCommentStatement("we have to append the current indent first."), new CodeConditionStatement(new CodeBinaryOperatorExpression(GenerationEnvironment.Ref().Prop("Length").VEquals(0.Prim()), CodeBinaryOperatorType.BooleanOr, endsWithNewline.Ref()), GenerationEnvironment.Ref().CallS("Append", currentIndent.Ref()), endsWithNewline.Ref().Assign(false.Prim())), new CodeCommentStatement("Check if the current text ends with a newline"), new CodeConditionStatement(textToAppend.Call("EndsWith", codeExpression, typeof(StringComparison).Expr().Prop("CurrentCulture")), new CodeAssignStatement(endsWithNewline.Ref(), true.Prim())), new CodeCommentStatement("This is an optimization. If the current indent is \"\", then we don't have to do any"), new CodeCommentStatement("of the more complex stuff further down."), new CodeConditionStatement(currentIndent.Ref().Prop("Length").VEquals(0.Prim()), GenerationEnvironment.Ref().CallS("Append", textToAppend), new CodeMethodReturnStatement()), new CodeCommentStatement("Everywhere there is a newline in the text, add an indent after it"), textToAppend.Assign(textToAppend.Call("Replace", codeExpression, codeExpression.Add(currentIndent.Ref()))), new CodeCommentStatement("If the text ends with a newline, then we should strip off the indent added at the very end"), new CodeCommentStatement("because the appropriate indent will be added when the next time Write() is called"), new CodeConditionStatement(endsWithNewline.Ref(), new CodeStatement[1]
			{
				GenerationEnvironment.Ref().CallS("Append", textToAppend, 0.Prim(), textToAppend.Prop("Length").Subtract(currentIndent.Ref().Prop("Length")))
			}, new CodeStatement[1]
			{
				GenerationEnvironment.Ref().CallS("Append", textToAppend)
			}));
			codeMemberMethod.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Transform-time helpers"));
			codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), textToAppend.VariableName));
			members.Add(codeMemberMethod);
		}

		private static void ProvideCurrentIndentProperty(CodeTypeMemberCollection members, CodeMemberField currentIndent)
		{
			CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
			codeMemberProperty.Type = new CodeTypeReference(typeof(string));
			codeMemberProperty.Name = "CurrentIndent";
			codeMemberProperty.AddSummaryComment("Gets the current indent we use when adding lines to the output");
			codeMemberProperty.Attributes = (MemberAttributes)24578;
			codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(currentIndent.Ref()));
			members.Add(codeMemberProperty);
		}

		private static void ProvideSessionProperty(CodeTypeMemberCollection members, CodeMemberField session)
		{
			CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
			codeMemberProperty.Type = new CodeTypeReference(typeof(IDictionary<string, object>));
			codeMemberProperty.Type.Options = CodeTypeReferenceOptions.GlobalReference;
			codeMemberProperty.Name = "Session";
			codeMemberProperty.AddSummaryComment("Current transformation session");
			codeMemberProperty.Attributes = MemberAttributes.Public;
			codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(session.Ref()));
			codeMemberProperty.SetStatements.Add(session.Ref().Assign(new CodePropertySetValueReferenceExpression()));
			codeMemberProperty.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "Properties"));
			members.Add(codeMemberProperty);
		}

		private static CodeMemberProperty ProvideIndentLengthsProperty(CodeTypeMemberCollection members, CodeMemberField indentLengthsField)
		{
			CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
			codeMemberProperty.Type = new CodeTypeReference(typeof(List<int>));
			codeMemberProperty.Name = "indentLengths";
			codeMemberProperty.AddSummaryComment("A list of the lengths of each indent that was added with PushIndent");
			codeMemberProperty.Attributes = MemberAttributes.Private;
			codeMemberProperty.GetStatements.Add(CodeDomHelpers.IfVariableNullThenInstantiateType(indentLengthsField.Ref(), typeof(List<int>)));
			codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(indentLengthsField.Ref()));
			members.Add(codeMemberProperty);
			return codeMemberProperty;
		}

		private static CodeMemberProperty ProvideErrorsProperty(CodeTypeMemberCollection members, CodeMemberField errors)
		{
			CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
			codeMemberProperty.Type = new CodeTypeReference(typeof(CompilerErrorCollection));
			codeMemberProperty.Name = "Errors";
			codeMemberProperty.AddSummaryComment("The error collection for the generation process");
			codeMemberProperty.Attributes = (MemberAttributes)24578;
			codeMemberProperty.GetStatements.Add(CodeDomHelpers.IfVariableNullThenInstantiateType(errors.Ref(), typeof(CompilerErrorCollection)));
			codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(errors.Ref()));
			members.Add(codeMemberProperty);
			return codeMemberProperty;
		}

		private static CodeMemberProperty ProvideGenerationEnvironmentProperty(CodeTypeMemberCollection members, CodeMemberField generationTimeBuilder)
		{
			CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
			codeMemberProperty.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Properties"));
			codeMemberProperty.Type = new CodeTypeReference(typeof(StringBuilder));
			codeMemberProperty.Name = "GenerationEnvironment";
			codeMemberProperty.AddSummaryComment("The string builder that generation-time code is using to assemble generated output");
			codeMemberProperty.Attributes = (MemberAttributes)12290;
			codeMemberProperty.GetStatements.Add(CodeDomHelpers.IfVariableNullThenInstantiateType(generationTimeBuilder.Ref(), typeof(StringBuilder)));
			codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(generationTimeBuilder.Ref()));
			codeMemberProperty.SetStatements.Add(new CodeAssignStatement(generationTimeBuilder.Ref(), new CodePropertySetValueReferenceExpression()));
			members.Add(codeMemberProperty);
			return codeMemberProperty;
		}
	}
}

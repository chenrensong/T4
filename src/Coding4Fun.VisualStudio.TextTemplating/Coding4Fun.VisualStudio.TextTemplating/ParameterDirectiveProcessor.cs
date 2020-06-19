using Coding4Fun.VisualStudio.TextTemplating.CodeDom;
using Coding4Fun.VisualStudio.TextTemplating.Properties;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	/// <summary>
	/// Directive processor to route simple serializable parameters from callers or the host to the template.
	/// </summary>
	public sealed class ParameterDirectiveProcessor : DirectiveProcessor, IRecognizeHostSpecific
	{
		/// <summary>
		/// Name of the directive provided by this processor ("parameter").
		/// </summary>
		internal const string DirectiveName = "parameter";

		/// <summary>
		/// The friendly name of this processor ("ParameterDirectiveProcessor").
		/// </summary>
		internal const string ProcessorName = "ParameterDirectiveProcessor";

		/// <summary>
		/// Whether the current processing run is host-specific
		/// </summary>
		private bool hostSpecific;

		/// <summary>
		/// Provider for the template's language
		/// </summary>
		private CodeDomProvider languageCodeDomProvider;

		/// <summary>
		/// Buffer that collates all of the code that instances of this directive processor need to run after base class initialization during a processing run
		/// </summary>
		private StringBuilder postInitializationBuffer;

		/// <summary>
		/// Buffer that collates all of the code that instances of this directive processor contribute during a processing run
		/// </summary>
		private StringBuilder codeBuffer;

		private static CodeGeneratorOptions StandardOptions => new CodeGeneratorOptions
		{
			BlankLinesBetweenMembers = true,
			IndentString = "    ",
			VerbatimOrder = true,
			BracingStyle = "C"
		};

		/// <summary>
		/// This processor does not require a host-specific template.
		/// </summary>
		public bool RequiresProcessingRunIsHostSpecific => false;

		/// <summary>
		/// Nothing to do as we complete our run.
		/// </summary>
		public override void FinishProcessingRun()
		{
		}

		/// <summary>
		/// Gets generated class code.
		/// </summary>
		/// <returns></returns>
		public override string GetClassCodeForProcessingRun()
		{
			return codeBuffer.ToString();
		}

		/// <summary>
		/// Get the code to contribute to the body of the initialize method of the generated
		/// template processing class as a consequence of the most recent run.
		/// This code will run after the base class' Initialize method
		/// </summary>
		/// <returns></returns>
		public override string GetPostInitializationCodeForProcessingRun()
		{
			return postInitializationBuffer.ToString();
		}

		public override string GetPreInitializationCodeForProcessingRun()
		{
			return string.Empty;
		}

		public override string[] GetReferencesForProcessingRun()
		{
			return new string[0];
		}

		public override string[] GetImportsForProcessingRun()
		{
			return new string[0];
		}

		/// <summary>
		/// Denote which properties are supported.
		/// </summary>
		/// <remarks>
		/// Only the "parameter" directive is supported.
		/// </remarks>
		/// <param name="directiveName"></param>
		/// <returns></returns>
		public override bool IsDirectiveSupported(string directiveName)
		{
			return StringComparer.OrdinalIgnoreCase.Compare(directiveName, "parameter") == 0;
		}

		public override void ProcessDirective(string directiveName, IDictionary<string, string> arguments)
		{
			if (!IsDirectiveSupported(directiveName))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.DirectiveNotSupported, directiveName), "directiveName");
			}
			if (!arguments.TryGetValue("name", out string value))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.DirectiveMissingArgument, directiveName, "name"), "arguments");
			}
			if (!arguments.TryGetValue("type", out string value2))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.DirectiveMissingArgument, directiveName, "type"), "arguments");
			}
			GenerateClassCode(value, value2, out CodeMemberField field);
			GeneratePostInitCode(value, value2, field);
		}

		/// <summary>
		/// Create the code for the Initialize method after its call to base.Initialize
		/// </summary>
		private void GeneratePostInitCode(string nameValue, string typeValue, CodeMemberField field)
		{
			CodeStatementCollection codeStatementCollection = new CodeStatementCollection();
			CodeVariableDeclarationStatement codeVariableDeclarationStatement = new CodeVariableDeclarationStatement(typeof(bool).Ref(), nameValue + "ValueAcquired", false.Prim());
			CodeVariableReferenceExpression codeVariableReferenceExpression = codeVariableDeclarationStatement.Ref();
			CodeTypeReference typeReference = new CodeTypeReference(typeValue, CodeTypeReferenceOptions.GlobalReference);
			CodeStatement setValueAcquired = codeVariableReferenceExpression.Assign(true.Prim());
			codeStatementCollection.Add(codeVariableDeclarationStatement);
			GenerateSessionLookup(nameValue, field, codeStatementCollection, setValueAcquired, typeReference);
			if (hostSpecific)
			{
				GenerateHostResolveParameterValueLookup(nameValue, field, codeStatementCollection, codeVariableReferenceExpression, typeReference, setValueAcquired, typeValue);
			}
			GenerateCallContextLookup(nameValue, field, codeStatementCollection, codeVariableReferenceExpression, typeReference, typeValue);
			CodeGeneratorOptions standardOptions = StandardOptions;
			using (StringWriter writer = new StringWriter(postInitializationBuffer, CultureInfo.InvariantCulture))
			{
				foreach (CodeStatement item in codeStatementCollection)
				{
					languageCodeDomProvider.GenerateCodeFromStatement(item, writer, standardOptions);
				}
			}
		}

		private static CodeStatement GenerateReportTypeMismatch(string nameValue, string typeValue)
		{
			return new CodeThisReferenceExpression().CallS("Error", string.Format(CultureInfo.CurrentCulture, Resources.ParameterDirectiveTypeMismatch, nameValue, typeValue).Prim());
		}

		private static void GenerateSessionLookup(string nameValue, CodeMemberField field, CodeStatementCollection statements, CodeStatement setValueAcquired, CodeTypeReference typeReference)
		{
			CodeExpression value = new CodeIndexerExpression(new CodeThisReferenceExpression().Prop("Session"), nameValue.Prim());
			statements.Add(new CodeConditionStatement(new CodeThisReferenceExpression().Prop("Session").Call("ContainsKey", nameValue.Prim()), field.Ref().Assign(typeReference.Cast(value)), setValueAcquired));
		}

		private static void GenerateCallContextLookup(string nameValue, CodeMemberField field, CodeStatementCollection statements, CodeVariableReferenceExpression valueAcquired, CodeTypeReference typeReference, string typeValue)
		{
			CodeVariableDeclarationStatement codeVariableDeclarationStatement = typeof(object).Decl("data", typeof(CallContext).Expr().Call("LogicalGetData", nameValue.Prim()));
			CodeVariableReferenceExpression codeVariableReferenceExpression = codeVariableDeclarationStatement.Ref();
			CodeAssignStatement codeAssignStatement = field.Ref().Assign(typeReference.Cast(codeVariableReferenceExpression));
			CodeConditionStatement codeConditionStatement = new CodeConditionStatement(codeVariableReferenceExpression.NotNull(), codeAssignStatement);
			statements.Add(new CodeConditionStatement(valueAcquired.VEquals(false.Prim()), codeVariableDeclarationStatement, codeConditionStatement));
		}

		private static void GenerateHostResolveParameterValueLookup(string nameValue, CodeMemberField field, CodeStatementCollection statements, CodeVariableReferenceExpression valueAcquired, CodeTypeReference typeReference, CodeStatement setValueAcquired, string typeValue)
		{
			CodePropertyReferenceExpression callSite = new CodeThisReferenceExpression().Prop("Host");
			CodeVariableDeclarationStatement codeVariableDeclarationStatement = new CodeVariableDeclarationStatement(typeof(string).Ref(), "parameterValue", callSite.Call("ResolveParameterValue", "Property".Prim(), "PropertyDirectiveProcessor".Prim(), nameValue.Prim()));
			CodeVariableReferenceExpression codeVariableReferenceExpression = codeVariableDeclarationStatement.Ref();
			CodeVariableDeclarationStatement codeVariableDeclarationStatement2 = new CodeVariableDeclarationStatement(typeof(TypeConverter).Ref(), "tc", typeof(TypeDescriptor).Expr().Call("GetConverter", new CodeTypeOfExpression(typeReference)));
			statements.Add(new CodeConditionStatement(valueAcquired.VEquals(false.Prim()), codeVariableDeclarationStatement, new CodeConditionStatement(typeof(string).Expr().Call("IsNullOrEmpty", codeVariableReferenceExpression).VEquals(false.Prim()), codeVariableDeclarationStatement2, new CodeConditionStatement(codeVariableDeclarationStatement2.Ref().NotNull().And(codeVariableDeclarationStatement2.Ref().Call("CanConvertFrom", new CodeTypeOfExpression(typeof(string).Ref()))), new CodeStatement[2]
			{
				field.Ref().Assign(new CodeCastExpression(typeReference, codeVariableDeclarationStatement2.Ref().Call("ConvertFrom", codeVariableReferenceExpression))),
				setValueAcquired
			}, new CodeStatement[1]
			{
				GenerateReportTypeMismatch(nameValue, typeValue)
			}))));
		}

		/// <summary>
		/// Create the code added as members to the transform class.
		/// </summary>
		private void GenerateClassCode(string nameValue, string typeValue, out CodeMemberField field)
		{
			field = new CodeMemberField(typeValue, string.Format(CultureInfo.InvariantCulture, "_{0}Field", nameValue));
			field.Attributes = MemberAttributes.Private;
			field.Type.Options = CodeTypeReferenceOptions.GlobalReference;
			CodeMemberProperty codeMemberProperty = new CodeMemberProperty();
			codeMemberProperty.Name = nameValue;
			codeMemberProperty.Type = new CodeTypeReference(typeValue, CodeTypeReferenceOptions.GlobalReference);
			codeMemberProperty.GetStatements.Add(new CodeMethodReturnStatement(field.Ref()));
			codeMemberProperty.AddSummaryComment("Access the " + nameValue + " parameter of the template.");
			CodeGeneratorOptions standardOptions = StandardOptions;
			using (StringWriter writer = new StringWriter(codeBuffer, CultureInfo.InvariantCulture))
			{
				languageCodeDomProvider.GenerateCodeFromMember(field, writer, standardOptions);
				languageCodeDomProvider.GenerateCodeFromMember(codeMemberProperty, writer, standardOptions);
			}
		}

		/// <summary>
		/// Starts processing run.
		/// </summary>
		/// <param name="languageProvider">Target language provider.</param>
		/// <param name="templateContents">The contents of the template being processed</param>
		/// <param name="errors">collection to report processing errors in</param>
		public override void StartProcessingRun(CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
		{
			if (languageProvider == null)
			{
				throw new ArgumentNullException("languageProvider");
			}
			base.StartProcessingRun(languageProvider, templateContents, errors);
			languageCodeDomProvider = languageProvider;
			postInitializationBuffer = new StringBuilder();
			codeBuffer = new StringBuilder();
		}

		/// <summary>
		/// Accept the host-specific value of the current processing run from the engine
		/// </summary>
		void IRecognizeHostSpecific.SetProcessingRunIsHostSpecific(bool isHostSpecific)
		{
			hostSpecific = isHostSpecific;
		}
	}
}

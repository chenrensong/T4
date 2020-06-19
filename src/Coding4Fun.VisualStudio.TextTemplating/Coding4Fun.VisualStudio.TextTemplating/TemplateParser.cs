using Coding4Fun.VisualStudio.TextTemplating.Properties;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Coding4Fun.VisualStudio.TextTemplating
{
	internal static class TemplateParser
	{
		private static readonly Regex templateParsingRegex = new Regex("\r\n\t\t\t# We check if we have an even number of \\ in the beginning of\t#\r\n\t\t\t# of the file preceeding an open tag. If we do, then it's\t\t#\r\n\t\t\t# boilerplate text. We check this in the very beginning because\t#\r\n\t\t\t# if not, the directives/classfeatures etc will match it and we\t#\r\n\t\t\t# won't get the initial backslashes as boilerplate code\t\t\t#\r\n\t\t\t(?<boilerplate>^(\\\\\\\\)+)(?=<\\#)|\r\n\r\n\t\t\t# Check for an unescaped (0 or even number of \\ preceeding\t\t#\r\n\t\t\t# it) directive start tag and it's accompanying end tag. Store\t#\r\n\t\t\t# text of the directive tag in a group named directive\t\t\t#\r\n\t\t\t(?<=([^\\\\]|^)(\\\\\\\\)*)<\\#@(?<directive>.*?)(?<=[^\\\\](\\\\\\\\)*)\\#>|\r\n\r\n\t\t\t# Check for an unescaped classfeature start tag and its end tag\t#\r\n\t\t\t# Store the text between the tags in group called classfeatures\t#\r\n\t\t\t(?<=([^\\\\]|^)(\\\\\\\\)*)<\\#\\+(?<classfeature>.*?)(?<=[^\\\\](\\\\\\\\)*)\\#>|\r\n\r\n\t\t\t# Check for an unescaped expression start tag and its end tag.\t#\r\n\t\t\t# Store the text between the tags in group called expression\t#\r\n\t\t\t(?<=([^\\\\]|^)(\\\\\\\\)*)<\\#=(?<expression>.*?)(?<=[^\\\\](\\\\\\\\)*)\\#>|\r\n\r\n\t\t\t# Check for an unescaped statement start tag and its end tag.\t#\r\n\t\t\t# Store the text between the tags in group called statement.\t#\r\n\t\t\t# We can only check for statements after checking for\t\t\t#\r\n\t\t\t# directives, expressions and classfeatures because the start\t#\r\n\t\t\t# tag for statements is a substring of the other start tags\t\t#\r\n\t\t\t(?<=([^\\\\]|^)(\\\\\\\\)*)<\\#(?<statement>.*?)(?<=[^\\\\](\\\\\\\\)*)\\#>|\r\n\r\n\t\t\t# Finally, check for boilerplate code that's not within a start\t#\r\n\t\t\t# or end tag (look for anything preceeding a start tag or an\t#\r\n\t\t\t# EOL) This has to be done at the ver end so that the .+ does\t#\r\n\t\t\t# not match other blocks\t\t\t\t\t\t\t\t\t\t#\r\n\t\t\t(?<boilerplate>.+?)(?=((?<=[^\\\\](\\\\\\\\)*)<\\#))|\r\n\t\t\t(?<boilerplate>.+)(?=$)", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

		private static readonly Regex directiveParsingRegex = new Regex("(?<pname>\\S+?)\\s*=\\s*\"(?<pvalue>.*?)(?<=[^\\\\](\\\\\\\\)*)\"|(?<name>\\S+)", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

		private static readonly Regex newlineFindingRegex = new Regex(Environment.NewLine, RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly Regex newlineAtLineStartRegex = new Regex("^" + Environment.NewLine, RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly Regex allNewlineRegex = new Regex("^(" + Environment.NewLine + ")*$", RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly Regex escapeFindingRegex = new Regex("\\\\+(?=<\\#)|\\\\+(?=\\#>)", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly Regex eolEscapeFindingRegex = new Regex("\\\\+(?=$)", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly Regex directiveEscapeFindingRegex = new Regex("\\\\+(?=\")|\\\\+(?=$)", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly Regex unescapedTagFindingRegex = new Regex("(^|[^\\\\])(\\\\\\\\)*(<\\#|\\#>)", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly Regex nameValidatingRegex = new Regex("^\\s*[\\w\\.]+\\s+", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly Regex paramValueValidatingRegex = new Regex("[\\w\\.]+\\s*=\\s*\"(.*?)(?<=[^\\\\](\\\\\\\\)*)\"\\s*", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline);

		/// <summary>
		/// A MatchEvaluator for replacing escape characters. 
		/// </summary>
		private static readonly MatchEvaluator escapeReplacingEvaluator = delegate(Match match)
		{
			if (match.Success && match.Value != null)
			{
				int length = (int)Math.Floor((double)match.Value.Length / 2.0);
				return match.Value.Substring(0, length);
			}
			return string.Empty;
		};

		public static List<Block> ParseTemplateIntoBlocks(string content, CompilerErrorCollection errors)
		{
			return ParseTemplateIntoBlocks(content, "", errors);
		}

		/// <summary>
		/// Parse a template file into blocks. Each block is of type boilerplate, directive, 
		/// statement, classfeature or expression. Also puts position information (line/column 
		/// number) for the block into each block. 
		/// </summary>
		/// <param name="content">Template content.</param>
		/// <param name="fileName"></param>
		/// <param name="errors">Error collection to report errors to.</param>
		/// <returns>Name of template file.</returns>
		public static List<Block> ParseTemplateIntoBlocks(string content, string fileName, CompilerErrorCollection errors)
		{
			if (content == null)
			{
				throw new ArgumentNullException("content");
			}
			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			if (errors == null)
			{
				throw new ArgumentNullException("errors");
			}
			List<Block> list = new List<Block>();
			MatchCollection matchCollection = templateParsingRegex.Matches(content);
			foreach (Match item in matchCollection)
			{
				Block block = new Block();
				Group group;
				if ((group = item.Groups["boilerplate"]).Success)
				{
					block.Type = BlockType.BoilerPlate;
				}
				else if ((group = item.Groups["directive"]).Success)
				{
					block.Type = BlockType.Directive;
				}
				else if ((group = item.Groups["classfeature"]).Success)
				{
					block.Type = BlockType.ClassFeature;
				}
				else if ((group = item.Groups["expression"]).Success)
				{
					block.Type = BlockType.Expression;
				}
				else if ((group = item.Groups["statement"]).Success)
				{
					block.Type = BlockType.Statement;
				}
				if (group != null && group.Success)
				{
					block.Text = group.Value;
					block.FileName = fileName;
					list.Add(block);
				}
			}
			InsertPositionInformation(list);
			WarnAboutUnexpectedTags(list, errors);
			StripEscapeCharacters(list);
			CheckBlockSequence(list, errors);
			return list;
		}

		/// <summary>
		/// Check to make sure that the blocks are in correct sequence i.e.
		/// * no statements after the first classfeature block, and
		/// * if the template contains a class block then it ends with a class block.
		/// If not, log errors. 
		/// </summary>
		private static void CheckBlockSequence(IList<Block> blocks, CompilerErrorCollection errors)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (Block block2 in blocks)
			{
				if (!flag)
				{
					if (block2.Type == BlockType.ClassFeature)
					{
						flag = true;
					}
				}
				else if (block2.Type == BlockType.Directive || block2.Type == BlockType.Statement)
				{
					CompilerError compilerError = new CompilerError(block2.FileName, block2.StartLineNumber, block2.StartColumnNumber, null, string.Format(CultureInfo.CurrentCulture, Resources.WrongBlockSequence, Enum.GetName(typeof(BlockType), block2.Type)));
					compilerError.IsWarning = false;
					errors.Add(compilerError);
					flag2 = true;
				}
			}
			if (flag && !flag2)
			{
				Block block = blocks[blocks.Count - 1];
				if (block.Type != BlockType.ClassFeature && (block.Type != BlockType.BoilerPlate || !allNewlineRegex.Match(block.Text).Success))
				{
					CompilerError compilerError2 = new CompilerError(block.FileName, block.StartLineNumber, block.StartColumnNumber, null, Resources.WrongFinalBlockType);
					compilerError2.IsWarning = false;
					errors.Add(compilerError2);
				}
			}
		}

		/// <summary>
		/// Parse a directive block for the directive name and param-value pairs
		/// </summary>
		/// <param name="block"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public static Directive ParseDirectiveBlock(Block block, CompilerErrorCollection errors)
		{
			if (block == null)
			{
				throw new ArgumentNullException("block");
			}
			if (errors == null)
			{
				throw new ArgumentNullException("errors");
			}
			if (!ValidateDirectiveString(block))
			{
				CompilerError compilerError = new CompilerError(block.FileName, block.StartLineNumber, block.StartColumnNumber, null, Resources.WrongDirectiveFormat);
				compilerError.IsWarning = false;
				errors.Add(compilerError);
				return null;
			}
			MatchCollection matchCollection = directiveParsingRegex.Matches(block.Text);
			string text = null;
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (Match item in matchCollection)
			{
				Group group;
				if ((group = item.Groups["name"]).Success)
				{
					text = group.Value;
				}
				else
				{
					string text2 = null;
					string text3 = null;
					if ((group = item.Groups["pname"]).Success)
					{
						text2 = group.Value;
					}
					if ((group = item.Groups["pvalue"]).Success)
					{
						text3 = group.Value;
					}
					if (text2 != null && text3 != null)
					{
						if (dictionary.ContainsKey(text2))
						{
							CompilerError compilerError2 = new CompilerError(block.FileName, block.StartLineNumber, block.StartColumnNumber, null, string.Format(CultureInfo.CurrentCulture, Resources.DuplicateDirectiveParameter, text2));
							compilerError2.IsWarning = true;
							errors.Add(compilerError2);
						}
						else
						{
							text3 = StripDirectiveEscapeCharacters(text3);
							dictionary.Add(text2, text3);
						}
					}
				}
			}
			if (text != null)
			{
				return new Directive(text, dictionary, block);
			}
			return null;
		}

		/// <summary>
		/// Validates a directive string and makes sure it is in the right format
		/// </summary>
		/// <returns>Whether the directive block is valid</returns>
		private static bool ValidateDirectiveString(Block block)
		{
			Match match = nameValidatingRegex.Match(block.Text);
			if (!match.Success)
			{
				return false;
			}
			int num = match.Length;
			MatchCollection matchCollection = paramValueValidatingRegex.Matches(block.Text);
			if (matchCollection.Count == 0)
			{
				return false;
			}
			foreach (Match item in matchCollection)
			{
				if (item.Index != num)
				{
					return false;
				}
				num += item.Length;
			}
			return num == block.Text.Length;
		}

		/// <summary>
		/// Insert position information (line and column number) into a block
		/// </summary>
		private static void InsertPositionInformation(IEnumerable<Block> blocks)
		{
			int num = 1;
			int num2 = 1;
			foreach (Block block in blocks)
			{
				if (block.Type == BlockType.ClassFeature || block.Type == BlockType.Directive || block.Type == BlockType.Expression)
				{
					num2 += 3;
				}
				else if (block.Type == BlockType.Statement)
				{
					num2 += 2;
				}
				block.StartLineNumber = num;
				block.StartColumnNumber = num2;
				MatchCollection matchCollection = newlineFindingRegex.Matches(block.Text);
				num += matchCollection.Count;
				num2 = ((matchCollection.Count <= 0) ? (num2 + block.Text.Length) : (block.Text.Length - matchCollection[matchCollection.Count - 1].Index - Environment.NewLine.Length + 1));
				block.EndLineNumber = num;
				block.EndColumnNumber = num2;
				if (block.Type != BlockType.BoilerPlate)
				{
					num2 += 2;
				}
			}
		}

		/// <summary>
		/// Remove unwanted newlines from the blocks.
		/// Also removes any boilerplate blocks that are empty (or that contained only new lines)
		/// </summary>
		internal static void StripExtraNewlines(List<Block> blocks)
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				if (block.Type == BlockType.BoilerPlate && i > 0 && blocks[i - 1].Type != BlockType.Expression && blocks[i - 1].Type != BlockType.BoilerPlate)
				{
					block.Text = newlineAtLineStartRegex.Replace(block.Text, string.Empty);
				}
				if (block.Type == BlockType.BoilerPlate && i > 0 && blocks[i - 1].Type == BlockType.ClassFeature && (i == blocks.Count - 1 || blocks[i + 1].Type == BlockType.ClassFeature))
				{
					block.Text = allNewlineRegex.Replace(block.Text, string.Empty);
				}
			}
			blocks.RemoveAll((Block b) => string.IsNullOrEmpty(b.Text));
		}

		/// <summary>
		/// Adds warnings to the error collection if unexpected unescaped start/end tags are found within the template
		/// </summary>
		private static void WarnAboutUnexpectedTags(IEnumerable<Block> blocks, CompilerErrorCollection errors)
		{
			foreach (Block block in blocks)
			{
				if (unescapedTagFindingRegex.Match(block.Text).Success)
				{
					CompilerError compilerError = new CompilerError(block.FileName, block.StartLineNumber, block.StartColumnNumber, null, Resources.UnexpectedTag);
					compilerError.IsWarning = false;
					errors.Add(compilerError);
				}
			}
		}

		/// <summary>
		/// Strips escape characters from the block text. 
		/// </summary>
		/// <param name="blocks"></param>
		private static void StripEscapeCharacters(IList<Block> blocks)
		{
			for (int i = 0; i < blocks.Count; i++)
			{
				Block block = blocks[i];
				block.Text = escapeFindingRegex.Replace(block.Text, escapeReplacingEvaluator);
				if (i != blocks.Count - 1)
				{
					block.Text = eolEscapeFindingRegex.Replace(block.Text, escapeReplacingEvaluator);
				}
			}
		}

		/// <summary>
		/// Strips escape characters before " in the directive strings
		/// </summary>
		/// <param name="valueString"></param>
		/// <returns></returns>
		private static string StripDirectiveEscapeCharacters(string valueString)
		{
			return directiveEscapeFindingRegex.Replace(valueString, escapeReplacingEvaluator);
		}
	}
}

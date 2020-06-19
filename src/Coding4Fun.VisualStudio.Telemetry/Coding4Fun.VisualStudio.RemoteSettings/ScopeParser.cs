using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class ScopeParser
	{
		internal class InvalidOperand : Operand
		{
			private string errorMessage;

			public InvalidOperand(string errorMessage)
			{
				this.errorMessage = errorMessage;
			}

			public override bool ToBool()
			{
				throw new ScopeParserException(errorMessage);
			}

			public override int CompareTo(Operand a)
			{
				throw new ScopeParserException(errorMessage);
			}
		}

		internal class BoolOperand : Operand
		{
			public BoolOperand(bool value)
			{
				Value = value;
			}

			public override bool ToBool()
			{
				return (bool)Value;
			}

			public override int CompareTo(Operand a)
			{
				if (a.Value is bool)
				{
					return ((bool)Value).CompareTo((bool)a.Value);
				}
				throw new ScopeParserException("Trying to compare two different types");
			}
		}

		internal class LazyOperand : Operand
		{
			private Lazy<Operand> lazy;

			public override object Value => lazy.Value.Value;

			public LazyOperand(Func<Operand> value)
			{
				lazy = new Lazy<Operand>(value);
			}

			public override int CompareTo(Operand a)
			{
				return lazy.Value.CompareTo(a);
			}

			public override bool ToBool()
			{
				return lazy.Value.ToBool();
			}
		}

		internal class StringOperand : Operand
		{
			public StringOperand(string value)
			{
				Value = value;
			}

			public override bool ToBool()
			{
				return Value != null;
			}

			public override int CompareTo(Operand a)
			{
				if (a.Value is string)
				{
					return ((string)Value).CompareTo((string)a.Value);
				}
				throw new ScopeParserException("Trying to compare two different types");
			}
		}

		internal class DoubleOperand : Operand
		{
			public DoubleOperand(double value)
			{
				Value = value;
			}

			public override bool ToBool()
			{
				return (double)Value != 0.0;
			}

			public override int CompareTo(Operand a)
			{
				if (a is DoubleOperand)
				{
					return ((double)Value).CompareTo((double)a.Value);
				}
				throw new ScopeParserException("Trying to compare two different types");
			}
		}

		internal abstract class Operand : IComparable<Operand>
		{
			public virtual object Value
			{
				get;
				set;
			}

			public abstract int CompareTo(Operand a);

			public bool Equals(Operand y)
			{
				return CompareTo(y) == 0;
			}

			public abstract bool ToBool();

			public static Operand operator &(Operand a, Operand b)
			{
				return new LazyOperand(() => new BoolOperand(a.ToBool() && b.ToBool()));
			}

			public static Operand operator |(Operand a, Operand b)
			{
				return new LazyOperand(() => new BoolOperand(a.ToBool() || b.ToBool()));
			}

			public static bool operator true(Operand a)
			{
				return a.ToBool();
			}

			public static bool operator false(Operand a)
			{
				return !a.ToBool();
			}

			public static bool operator !(Operand a)
			{
				return !a.ToBool();
			}
		}

		internal struct Operator
		{
			internal delegate Operand FuncDelegate(Operand a, Operand b);

			internal enum Id
			{
				Non,
				Opn,
				Cls,
				Not,
				And,
				Or,
				Eq,
				Neq,
				Gt,
				Lt,
				Gte,
				Lte
			}

			internal enum Fixity
			{
				Left,
				Right
			}

			internal enum Type
			{
				Unary,
				Binary
			}

			internal Id IdValue;

			internal ushort Priority;

			internal Fixity FixityValue;

			internal Type TypeValue;

			internal FuncDelegate TheFuncDelegate;

			internal static readonly FuncDelegate Non = (Operand a, Operand b) => new BoolOperand(false);

			internal static readonly FuncDelegate Not = (Operand a, Operand b) => new BoolOperand(!a);

			internal static readonly FuncDelegate Gt = (Operand a, Operand b) => new BoolOperand(a.CompareTo(b) > 0);

			internal static readonly FuncDelegate Lt = (Operand a, Operand b) => new BoolOperand(a.CompareTo(b) < 0);

			internal static readonly FuncDelegate Gte = (Operand a, Operand b) => new BoolOperand(a.CompareTo(b) >= 0);

			internal static readonly FuncDelegate Lte = (Operand a, Operand b) => new BoolOperand(a.CompareTo(b) <= 0);

			internal static readonly FuncDelegate Eq = (Operand a, Operand b) => new BoolOperand(a.Equals(b));

			internal static readonly FuncDelegate Neq = (Operand a, Operand b) => new BoolOperand(!a.Equals(b));

			internal static readonly FuncDelegate And = (Operand a, Operand b) => a && b;

			internal static readonly FuncDelegate Or = (Operand a, Operand b) => a || b;

			internal static readonly IDictionary<string, Operator> StrMap;

			static Operator()
			{
				Dictionary<string, Operator> dictionary = new Dictionary<string, Operator>();
				Operator value = new Operator
				{
					IdValue = Id.Opn,
					Priority = 0,
					FixityValue = Fixity.Left,
					TypeValue = Type.Unary,
					TheFuncDelegate = Non
				};
				dictionary.Add("(", value);
				value = new Operator
				{
					IdValue = Id.Cls,
					Priority = 0,
					FixityValue = Fixity.Left,
					TypeValue = Type.Unary,
					TheFuncDelegate = Non
				};
				dictionary.Add(")", value);
				value = new Operator
				{
					IdValue = Id.Not,
					Priority = 80,
					FixityValue = Fixity.Right,
					TypeValue = Type.Unary,
					TheFuncDelegate = Not
				};
				dictionary.Add("!", value);
				value = new Operator
				{
					IdValue = Id.Gt,
					Priority = 64,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegate = Gt
				};
				dictionary.Add(">", value);
				value = new Operator
				{
					IdValue = Id.Lt,
					Priority = 64,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegate = Lt
				};
				dictionary.Add("<", value);
				value = new Operator
				{
					IdValue = Id.Gte,
					Priority = 64,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegate = Gte
				};
				dictionary.Add(">=", value);
				value = new Operator
				{
					IdValue = Id.Lte,
					Priority = 64,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegate = Lte
				};
				dictionary.Add("<=", value);
				value = new Operator
				{
					IdValue = Id.Eq,
					Priority = 48,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegate = Eq
				};
				dictionary.Add("==", value);
				value = new Operator
				{
					IdValue = Id.Neq,
					Priority = 48,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegate = Neq
				};
				dictionary.Add("!=", value);
				value = new Operator
				{
					IdValue = Id.And,
					Priority = 32,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegate = And
				};
				dictionary.Add("&&", value);
				value = new Operator
				{
					IdValue = Id.Or,
					Priority = 16,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegate = Or
				};
				dictionary.Add("||", value);
				StrMap = dictionary;
			}
		}

		private readonly string expression;

		private readonly Stack<Operand> output = new Stack<Operand>();

		private readonly Stack<Operator> operators = new Stack<Operator>();

		private readonly IDictionary<string, IScopeFilterProvider> providedFilters;

		private readonly Regex stringRegex = new Regex("'(.*)?'");

		private int expressionIndex;

		internal ScopeParser(string expression, IDictionary<string, IScopeFilterProvider> providedFilters)
		{
			CodeContract.RequiresArgumentNotNull<string>(expression, "expression");
			CodeContract.RequiresArgumentNotNull<IDictionary<string, IScopeFilterProvider>>(providedFilters, "providedFilters");
			this.expression = expression;
			this.providedFilters = providedFilters;
		}

		public bool Run()
		{
			while (expressionIndex < expression.Length)
			{
				switch (expression[expressionIndex])
				{
				case '\t':
				case ' ':
					expressionIndex++;
					break;
				case '!':
				case '&':
				case '(':
				case ')':
				case '<':
				case '=':
				case '>':
				case '|':
					ParseOperator(ScanOperator());
					break;
				default:
					ParseOperand(ScanOperand());
					break;
				}
			}
			while (operators.Count != 0)
			{
				EvaluateOutput();
			}
			if (output.Count != 1)
			{
				throw new ScopeParserException("Operand and operator count mismatch");
			}
			return output.Peek().ToBool();
		}

		private void ParseOperator(Operator op)
		{
			if (op.IdValue == Operator.Id.Opn)
			{
				operators.Push(op);
			}
			else if (op.IdValue == Operator.Id.Cls)
			{
				while (operators.Count != 0 && operators.Peek().IdValue != Operator.Id.Opn)
				{
					EvaluateOutput();
				}
				if (operators.Count != 0)
				{
					operators.Pop();
				}
			}
			else
			{
				while (operators.Count != 0 && (op.Priority < operators.Peek().Priority || (op.Priority == operators.Peek().Priority && op.FixityValue == Operator.Fixity.Left)))
				{
					EvaluateOutput();
				}
				operators.Push(op);
			}
		}

		private void ParseOperand(Operand op)
		{
			output.Push(op);
		}

		private Operator ScanOperator()
		{
			string text = expression[expressionIndex++].ToString();
			if (expressionIndex < expression.Length)
			{
				char c = expression[expressionIndex];
				if (c == '=' || c == '&' || c == '|')
				{
					text += c.ToString();
					expressionIndex++;
				}
			}
			if (!Operator.StrMap.TryGetValue(text, out Operator value))
			{
				throw new ScopeParserException("Invalid operator: " + text);
			}
			return value;
		}

		private Operand ScanOperand()
		{
			Func<char, bool> isAlphabet = (char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
			Func<char, bool> isNumeral = (char n) => n >= '0' && n <= '9';
			Func<char, bool> isSpecial = (char c) => c == '.' || c == '_' || c == '\'';
			Func<char, bool> predicate = (char c) => isAlphabet(c) || isNumeral(c) || isSpecial(c);
			string text = new string(expression.Skip(expressionIndex).TakeWhile(predicate).ToArray());
			expressionIndex += text.Length;
			if (text.Length == 0)
			{
				throw new ScopeParserException("Missing operand: " + GetRestOfExpression());
			}
			if (double.TryParse(text, out double result))
			{
				return new DoubleOperand(result);
			}
			Match match = stringRegex.Match(text);
			if (match.Success)
			{
				return new StringOperand(match.Groups[1].Value);
			}
			string subkey = null;
			string[] array = text.Split('.');
			string text2;
			if (array.Length == 2)
			{
				text2 = array[0];
				subkey = array[1];
			}
			else
			{
				text2 = text;
			}
			if (providedFilters.TryGetValue(text2, out IScopeFilterProvider value))
			{
				ISingleValueScopeFilterProvider<ScopeValue> singleValue = value as ISingleValueScopeFilterProvider<ScopeValue>;
				if (singleValue != null)
				{
					if (subkey != null)
					{
						throw new ScopeParserException("Filter has subkey, but only single-value provider");
					}
					return new LazyOperand(() => singleValue.Provide().GetOperand());
				}
				IMultiValueScopeFilterProvider<ScopeValue> multiValue = value as IMultiValueScopeFilterProvider<ScopeValue>;
				if (multiValue != null)
				{
					if (subkey == null)
					{
						throw new ScopeParserException("Filter has no subkey, yet multi-value provider");
					}
					return new LazyOperand(() => multiValue.Provide(subkey).GetOperand());
				}
			}
			return new InvalidOperand("Could not find provided scope with name: " + text2);
		}

		private void EvaluateOutput()
		{
			if (operators.Count == 0)
			{
				throw new ScopeParserException("Missing operator(s): " + GetRestOfExpression());
			}
			Operator @operator = operators.Pop();
			if (@operator.TypeValue == Operator.Type.Unary && output.Count >= 1)
			{
				Operand operand = output.Pop();
				output.Push(@operator.TheFuncDelegate(operand, operand));
				return;
			}
			if (@operator.TypeValue == Operator.Type.Binary && output.Count >= 2)
			{
				Operand b = output.Pop();
				Operand a = output.Pop();
				output.Push(@operator.TheFuncDelegate(a, b));
				return;
			}
			throw new ScopeParserException("Missing operand(s): " + GetRestOfExpression());
		}

		private string GetRestOfExpression()
		{
			return expression.Substring(expressionIndex);
		}
	}
}

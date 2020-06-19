using Coding4Fun.VisualStudio.Utilities.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Coding4Fun.VisualStudio.RemoteSettings
{
	internal class AsyncScopeParser
	{
		internal class InvalidOperand : AsyncOperand
		{
			private string errorMessage;

			public InvalidOperand(string errorMessage)
			{
				this.errorMessage = errorMessage;
			}

			public override Task<bool> ToBoolAsync()
			{
				throw new ScopeParserException(errorMessage);
			}

			public override Task<int> CompareToAsync(AsyncOperand a)
			{
				throw new ScopeParserException(errorMessage);
			}
		}

		internal class BoolOperand : AsyncOperand
		{
			public BoolOperand(bool value)
			{
				base.value = Task.FromResult((object)value);
			}

			public override async Task<bool> ToBoolAsync()
			{
				return (bool)(await Value().ConfigureAwait(false));
			}

			public override async Task<int> CompareToAsync(AsyncOperand a)
			{
				object aValue = await a.Value().ConfigureAwait(false);
				if (aValue is bool)
				{
					return ((bool)(await Value().ConfigureAwait(false))).CompareTo((bool)aValue);
				}
				throw new ScopeParserException("Trying to compare two different types");
			}
		}

		internal class LazyOperand : AsyncOperand
		{
			private Lazy<Task<AsyncOperand>> lazy;

			public override async Task<object> Value()
			{
				return await(await lazy.Value.ConfigureAwait(false)).Value().ConfigureAwait(false);
			}

			public LazyOperand(Func<Task<AsyncOperand>> value)
			{
				lazy = new Lazy<Task<AsyncOperand>>(value);
			}

			public LazyOperand(Func<AsyncOperand> value)
			{
				lazy = new Lazy<Task<AsyncOperand>>(() => Task.FromResult(value()));
			}

			public override async Task<int> CompareToAsync(AsyncOperand a)
			{
				return await(await lazy.Value).CompareToAsync(a).ConfigureAwait(false);
			}

			public override async Task<bool> ToBoolAsync()
			{
				return await(await lazy.Value).ToBoolAsync().ConfigureAwait(false);
			}
		}

		internal class StringOperand : AsyncOperand
		{
			public StringOperand(string value)
			{
				base.value = Task.FromResult((object)value);
			}

			public override async Task<bool> ToBoolAsync()
			{
				return await Value().ConfigureAwait(false) != null;
			}

			public override async Task<int> CompareToAsync(AsyncOperand a)
			{
				object aValue = await a.Value().ConfigureAwait(false);
				if (aValue is string)
				{
					return ((string)(await Value().ConfigureAwait(false))).CompareTo((string)aValue);
				}
				throw new ScopeParserException("Trying to compare two different types");
			}
		}

		internal class DoubleOperand : AsyncOperand
		{
			public DoubleOperand(double value)
			{
				base.value = Task.FromResult((object)value);
			}

			public override async Task<bool> ToBoolAsync()
			{
				return (double)(await Value().ConfigureAwait(false)) != 0.0;
			}

			public override async Task<int> CompareToAsync(AsyncOperand a)
			{
				object aValue = await a.Value().ConfigureAwait(false);
				if (a is DoubleOperand)
				{
					return ((double)(await Value().ConfigureAwait(false))).CompareTo(aValue);
				}
				throw new ScopeParserException("Trying to compare two different types");
			}
		}

		internal abstract class AsyncOperand
		{
			protected Task<object> value;

			public virtual Task<object> Value()
			{
				return value;
			}

			public abstract Task<int> CompareToAsync(AsyncOperand a);

			public async Task<bool> EqualsAsync(AsyncOperand y)
			{
				return await CompareToAsync(y).ConfigureAwait(false) == 0;
			}

			public abstract Task<bool> ToBoolAsync();

			public static AsyncOperand AndAsync(AsyncOperand a, AsyncOperand b)
			{
				return new LazyOperand(async () => (await a.ToBoolAsync().ConfigureAwait(false) && await b.ToBoolAsync().ConfigureAwait(false)) ? new BoolOperand(true) : new BoolOperand(false));
			}

			public static AsyncOperand OrAsync(AsyncOperand a, AsyncOperand b)
			{
				return new LazyOperand(async delegate
				{
					if (await a.ToBoolAsync().ConfigureAwait(false))
					{
						return new BoolOperand(true);
					}
					return (await b.ToBoolAsync().ConfigureAwait(false)) ? new BoolOperand(true) : new BoolOperand(false);
				});
			}

			public static async Task<AsyncOperand> Not(AsyncOperand a)
			{
				return new BoolOperand(!(await a.ToBoolAsync().ConfigureAwait(false)));
			}
		}

		internal struct Operator
		{
			internal delegate Task<AsyncOperand> FuncDelegateAsync(AsyncOperand a, AsyncOperand b);

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

			internal FuncDelegateAsync TheFuncDelegateAsync;

			internal static readonly FuncDelegateAsync Non = (AsyncOperand a, AsyncOperand b) => Task.FromResult((AsyncOperand)new BoolOperand(false));

			internal static readonly FuncDelegateAsync Not = (AsyncOperand a, AsyncOperand b) => AsyncOperand.Not(a);

			internal static readonly FuncDelegateAsync Gt = async (AsyncOperand a, AsyncOperand b) => new BoolOperand(await a.CompareToAsync(b).ConfigureAwait(false) > 0);

			internal static readonly FuncDelegateAsync Lt = async (AsyncOperand a, AsyncOperand b) => new BoolOperand(await a.CompareToAsync(b).ConfigureAwait(false) < 0);

			internal static readonly FuncDelegateAsync Gte = async (AsyncOperand a, AsyncOperand b) => new BoolOperand(await a.CompareToAsync(b).ConfigureAwait(false) >= 0);

			internal static readonly FuncDelegateAsync Lte = async (AsyncOperand a, AsyncOperand b) => new BoolOperand(await a.CompareToAsync(b).ConfigureAwait(false) <= 0);

			internal static readonly FuncDelegateAsync Eq = async (AsyncOperand a, AsyncOperand b) => new BoolOperand(await a.EqualsAsync(b).ConfigureAwait(false));

			internal static readonly FuncDelegateAsync Neq = async (AsyncOperand a, AsyncOperand b) => new BoolOperand(!(await a.EqualsAsync(b).ConfigureAwait(false)));

			internal static readonly FuncDelegateAsync And = (AsyncOperand a, AsyncOperand b) => Task.FromResult(AsyncOperand.AndAsync(a, b));

			internal static readonly FuncDelegateAsync Or = (AsyncOperand a, AsyncOperand b) => Task.FromResult(AsyncOperand.OrAsync(a, b));

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
					TheFuncDelegateAsync = Non
				};
				dictionary.Add("(", value);
				value = new Operator
				{
					IdValue = Id.Cls,
					Priority = 0,
					FixityValue = Fixity.Left,
					TypeValue = Type.Unary,
					TheFuncDelegateAsync = Non
				};
				dictionary.Add(")", value);
				value = new Operator
				{
					IdValue = Id.Not,
					Priority = 80,
					FixityValue = Fixity.Right,
					TypeValue = Type.Unary,
					TheFuncDelegateAsync = Not
				};
				dictionary.Add("!", value);
				value = new Operator
				{
					IdValue = Id.Gt,
					Priority = 64,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegateAsync = Gt
				};
				dictionary.Add(">", value);
				value = new Operator
				{
					IdValue = Id.Lt,
					Priority = 64,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegateAsync = Lt
				};
				dictionary.Add("<", value);
				value = new Operator
				{
					IdValue = Id.Gte,
					Priority = 64,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegateAsync = Gte
				};
				dictionary.Add(">=", value);
				value = new Operator
				{
					IdValue = Id.Lte,
					Priority = 64,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegateAsync = Lte
				};
				dictionary.Add("<=", value);
				value = new Operator
				{
					IdValue = Id.Eq,
					Priority = 48,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegateAsync = Eq
				};
				dictionary.Add("==", value);
				value = new Operator
				{
					IdValue = Id.Neq,
					Priority = 48,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegateAsync = Neq
				};
				dictionary.Add("!=", value);
				value = new Operator
				{
					IdValue = Id.And,
					Priority = 32,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegateAsync = And
				};
				dictionary.Add("&&", value);
				value = new Operator
				{
					IdValue = Id.Or,
					Priority = 16,
					FixityValue = Fixity.Left,
					TypeValue = Type.Binary,
					TheFuncDelegateAsync = Or
				};
				dictionary.Add("||", value);
				StrMap = dictionary;
			}
		}

		private readonly string expression;

		private readonly Stack<AsyncOperand> output = new Stack<AsyncOperand>();

		private readonly Stack<Operator> operators = new Stack<Operator>();

		private readonly IDictionary<string, IScopeFilterProvider> providedFilters;

		private readonly Regex stringRegex = new Regex("'(.*)?'");

		private int expressionIndex;

		internal AsyncScopeParser(string expression, IDictionary<string, IScopeFilterProvider> providedFilters)
		{
			CodeContract.RequiresArgumentNotNull<string>(expression, "expression");
			CodeContract.RequiresArgumentNotNull<IDictionary<string, IScopeFilterProvider>>(providedFilters, "providedFilters");
			this.expression = expression;
			this.providedFilters = providedFilters;
		}

		public async Task<bool> RunAsync()
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
					await ParseOperatorAsync(ScanOperator());
					break;
				default:
					ParseOperand(ScanOperand());
					break;
				}
			}
			while (operators.Count != 0)
			{
				await EvaluateOutputAsync().ConfigureAwait(false);
			}
			if (output.Count != 1)
			{
				throw new ScopeParserException("Operand and operator count mismatch");
			}
			return await output.Peek().ToBoolAsync().ConfigureAwait(false);
		}

		private async Task ParseOperatorAsync(Operator op)
		{
			if (op.IdValue == Operator.Id.Opn)
			{
				operators.Push(op);
			}
			else if (op.IdValue == Operator.Id.Cls)
			{
				while (operators.Count != 0 && operators.Peek().IdValue != Operator.Id.Opn)
				{
					await EvaluateOutputAsync().ConfigureAwait(false);
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
					await EvaluateOutputAsync().ConfigureAwait(false);
				}
				operators.Push(op);
			}
		}

		private void ParseOperand(AsyncOperand op)
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

		private AsyncOperand ScanOperand()
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
					LazyOperand lazyOperand = CastAppropiateSingle(value);
					if (lazyOperand != null)
					{
						return lazyOperand;
					}
					return new LazyOperand(() => singleValue.Provide().GetAsyncOperand());
				}
				IMultiValueScopeFilterProvider<ScopeValue> multiValue = value as IMultiValueScopeFilterProvider<ScopeValue>;
				if (multiValue != null)
				{
					if (subkey == null)
					{
						throw new ScopeParserException("Filter has no subkey, yet multi-value provider");
					}
					LazyOperand lazyOperand2 = CastAppropiateMulti(value, subkey);
					if (lazyOperand2 != null)
					{
						return lazyOperand2;
					}
					return new LazyOperand(() => multiValue.Provide(subkey).GetAsyncOperand());
				}
			}
			return new InvalidOperand("Could not find provided scope with name: " + text2);
		}

		private async Task EvaluateOutputAsync()
		{
			if (operators.Count == 0)
			{
				throw new ScopeParserException("Missing operator(s): " + GetRestOfExpression());
			}
			Operator @operator = operators.Pop();
			if (@operator.TypeValue == Operator.Type.Unary && output.Count >= 1)
			{
				AsyncOperand asyncOperand = output.Pop();
				Stack<AsyncOperand> stack = output;
				stack.Push(await @operator.TheFuncDelegateAsync(asyncOperand, asyncOperand));
				return;
			}
			if (@operator.TypeValue == Operator.Type.Binary && output.Count >= 2)
			{
				AsyncOperand b = output.Pop();
				AsyncOperand a = output.Pop();
				Stack<AsyncOperand> stack = output;
				stack.Push(await @operator.TheFuncDelegateAsync(a, b));
				return;
			}
			throw new ScopeParserException("Missing operand(s): " + GetRestOfExpression());
		}

		private string GetRestOfExpression()
		{
			return expression.Substring(expressionIndex);
		}

		private static LazyOperand CastAppropiateSingle(IScopeFilterProvider provider)
		{
			ISingleValueScopeFilterAsyncProvider<BoolScopeValue> result = provider as ISingleValueScopeFilterAsyncProvider<BoolScopeValue>;
			if (result != null)
			{
				return new LazyOperand(async () => (await result.ProvideAsync().ConfigureAwait(false)).GetAsyncOperand());
			}
			ISingleValueScopeFilterAsyncProvider<DoubleScopeValue> result2 = provider as ISingleValueScopeFilterAsyncProvider<DoubleScopeValue>;
			if (result2 != null)
			{
				return new LazyOperand(async () => (await result2.ProvideAsync().ConfigureAwait(false)).GetAsyncOperand());
			}
			ISingleValueScopeFilterAsyncProvider<StringScopeValue> result3 = provider as ISingleValueScopeFilterAsyncProvider<StringScopeValue>;
			if (result3 != null)
			{
				return new LazyOperand(async () => (await result3.ProvideAsync().ConfigureAwait(false)).GetAsyncOperand());
			}
			return null;
		}

		private static LazyOperand CastAppropiateMulti(IScopeFilterProvider provider, string subkey)
		{
			IMultiValueScopeFilterAsyncProvider<BoolScopeValue> result = provider as IMultiValueScopeFilterAsyncProvider<BoolScopeValue>;
			if (result != null)
			{
				return new LazyOperand(async () => (await result.ProvideAsync(subkey).ConfigureAwait(false)).GetAsyncOperand());
			}
			IMultiValueScopeFilterAsyncProvider<DoubleScopeValue> result2 = provider as IMultiValueScopeFilterAsyncProvider<DoubleScopeValue>;
			if (result2 != null)
			{
				return new LazyOperand(async () => (await result2.ProvideAsync(subkey).ConfigureAwait(false)).GetAsyncOperand());
			}
			IMultiValueScopeFilterAsyncProvider<StringScopeValue> result3 = provider as IMultiValueScopeFilterAsyncProvider<StringScopeValue>;
			if (result3 != null)
			{
				return new LazyOperand(async () => (await result3.ProvideAsync(subkey).ConfigureAwait(false)).GetAsyncOperand());
			}
			return null;
		}
	}
}

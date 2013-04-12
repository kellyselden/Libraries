using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KellySelden.Libraries.Extensions;

namespace KellySelden.Libraries.Expressions
{
	public class Expression
	{
		const string ExceptionMessage = "filter expression parentheses do not match up";

		readonly StringComparison _comparisonType;
		readonly bool _ignoreWhitespace;

		public Expression(StringComparison comparisonType = StringComparison.CurrentCulture, bool ignoreWhitespace = false)
		{
			_comparisonType = comparisonType;
			_ignoreWhitespace = ignoreWhitespace;
		}

		public T EvaluateExpression<T>(string expression, string[][] operators, Dictionary<string, T> valueLookup, Func<T, T, string, T> operation)
		{
			return EvaluateTree(ParseExpression(expression, operators), valueLookup, operation);
		}

		public IExpressionNode ParseExpression(string expression, string[][] operators)
		{
			expression = _ignoreWhitespace ? expression.Replace(" ", "") : Regex.Replace(expression, "\\s+", " ").Replace("( ", "(").Replace(" )", ")").Trim();

			return ParseExpressionRecursive(expression, operators.Reverse().ToArray(),
				operators.Aggregate((cur, next) => cur.Union(next).ToArray()));
		}
		IExpressionNode ParseExpressionRecursive(string expression, string[][] operators, string[] operatorsFlattened)
		{
			if (operatorsFlattened.All(op => !expression.Contains(op, _comparisonType)))
			{
				return new ExpressionLeaf { Expression = expression };
			}

			Dictionary<int, string> topLevelParentheses;
			string @operator;
			expression = AddImplicitParentheses(expression, operators, out topLevelParentheses, out @operator);

			var left = topLevelParentheses.First();
			var right = topLevelParentheses.Last();
			return new ExpressionBranch
			{
				Left = ParseExpressionRecursive(left.Value, operators, operatorsFlattened),
				Right = ParseExpressionRecursive(right.Value, operators, operatorsFlattened),
				Operator = @operator,
				Expression = expression
			};
		}

		string AddImplicitParentheses(string expression, string[][] operators, out Dictionary<int, string> topLevelParentheses, out string @operator)
		{
			while (true)
			{
				var leftIndexes = DictionaryExtensions.ToDictionary(expression.IndexOfAll('('), i => '(');
				var rightIndexes = DictionaryExtensions.ToDictionary(expression.IndexOfAll(')'), i => ')');
				if (leftIndexes.Count != rightIndexes.Count)
				{
					throw new InvalidOperationException(ExceptionMessage);
				}
				if (leftIndexes.Count == 1 && leftIndexes.Single().Key == 0 && rightIndexes.Single().Key == expression.Length - 1)
				{
					expression = expression.Substring(1, expression.Length - 2);
					continue;
				}

				topLevelParentheses = new Dictionary<int, string>();
				string maskedExpression = expression; //needed so splitting on spaces skips parentheses
				int count = 0, leftStartIndex = 0;
				foreach (KeyValuePair<int, char> index in leftIndexes.Union(rightIndexes).OrderBy(p => p.Key))
				{
					if (index.Value == ')' && count == 0)
					{
						throw new InvalidOperationException(ExceptionMessage);
					}
					if (index.Value == '(')
					{
						if (count++ == 0)
						{
							leftStartIndex = index.Key + 1;
						}
					}
					else if (--count == 0)
					{
						int length = index.Key - leftStartIndex;
						string substring = expression.Substring(leftStartIndex, length);
						topLevelParentheses.Add(leftStartIndex, substring);
						maskedExpression = maskedExpression.Replace(substring, "".PadRight(length, 'x'));
					}
				}

				@operator = null;
				bool cont = false;
				foreach (string[] operatorGroup in operators)
				{
					IEnumerable<string> newOperatorGroup = _ignoreWhitespace ? operatorGroup : operatorGroup.Select(op => ' ' + op + ' ');
					KeyValuePair<string, string>[] split = maskedExpression.SplitWithSeparator(newOperatorGroup, _comparisonType).ToArray();
					for (int i = 0, index = 0; i < split.Length; i++)
					{
						if (split[i].Value != null)
							@operator = split[i].Value.Trim();
						string separator = split[i].Value ?? "";
						string subexpression = split[i].Key;
						if (subexpression == maskedExpression) break;
						int length = subexpression.Length;
						foreach (KeyValuePair<int, string> kvp in topLevelParentheses) //unmasking expression
						{
							if (kvp.Key >= index && kvp.Key < index + length)
							{
								subexpression = subexpression.Replace(subexpression.Substring(kvp.Key - index, kvp.Value.Length), kvp.Value);
							}
						}
						if (subexpression.First() != '(' || subexpression.Last() != ')')
						{
							subexpression = '(' + subexpression + ')';
							cont = true;
						}
						if (split.Length > 2)
						{
							if (i == 0)
								subexpression = "".PadLeft(split.Length - 2, '(') + subexpression;
							else if (i != split.Length - 1)
								subexpression += ')';
							cont = true;
						}
						index += split[i].Key.Length + separator.Length;
						split[i] = new KeyValuePair<string, string>(subexpression, separator);
					}
					if (cont)
					{
						var sb = new StringBuilder();
						foreach (KeyValuePair<string, string> kvp in split)
						{
							sb.Append(kvp.Key).Append(kvp.Value);
						}
						expression = sb.ToString();
						break;
					}
				}
				if (!cont) break;
			}
			return expression;
		}

		public T EvaluateTree<T>(IExpressionNode node, Dictionary<string, T> valueLookup, Func<T, T, string, T> operation)
		{
			string[] operands = valueLookup.Select(v => v.Key).ToArray();
			if (HasDuplicates(operands))
				throw new InvalidOperationException("valueLookup has duplicates");
			if (IsValueUnused(node.Expression, operands))
				throw new InvalidOperationException("valueLookup has unused values");
			if (IsMissingValues(node, operands))
				throw new InvalidOperationException("valueLookup is missing values");
			return EvaluateTreeRecursive(node, valueLookup, operation);
		}
		T EvaluateTreeRecursive<T>(IExpressionNode node, Dictionary<string, T> valueLookup, Func<T, T, string, T> operation)
		{
			Func<IExpressionNode, T> getOrRecurse = n =>
			{
				if (valueLookup.Keys.All(operand => !operand.Equals(n.Expression, _comparisonType)))
				{
					//adding to lookup to short-circuit duplicate subexpressions, preserving original collection
					valueLookup = new Dictionary<string, T>(valueLookup) { { n.Expression, EvaluateTreeRecursive(n, valueLookup, operation) } };
				}
				return valueLookup.Single(v => v.Key.Equals(n.Expression, _comparisonType)).Value;
			};

			var branch = node as ExpressionBranch;
			if (branch == null)
				return getOrRecurse(node);
			return operation(getOrRecurse(branch.Left), getOrRecurse(branch.Right), branch.Operator);
		}

		public bool HasDuplicates(string[] operands)
		{
			var alreadyChecked = new List<string>();
			foreach (string operand in operands)
			{
				if (alreadyChecked.Any(s => s.Contains(operand, _comparisonType)))
					return true;
				alreadyChecked.Add(operand);
			}
			return false;
		}

		public bool IsValueUnused(string expression, string[] operands)
		{
			return operands.Any(operand => !expression.Contains(operand, _comparisonType));
		}

		public bool IsMissingValues(IExpressionNode node, string[] operands)
		{
			var branch = node as ExpressionBranch;
			if (branch == null)
				return operands.All(operand => !node.Expression.Contains(operand, _comparisonType));
			return IsMissingValues(branch.Left, operands)
				|| IsMissingValues(branch.Right, operands);
		}
	}
}
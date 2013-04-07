using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellySelden.Libraries.Extensions;

namespace KellySelden.Libraries.Expressions
{
	public static class Expression
	{
		const string ExceptionMessage = "filter expression parentheses do not match up";

		public static T EvaluateExpression<T>(string expression, string[][] operators, Dictionary<string, T> valueLookup, Func<T, T, string, T> operation)
		{
			return EvaluateTree(ParseExpression(expression, operators), valueLookup, operation);
		}
		
		public static ExpressionBranch ParseExpression(string expression, string[][] operators)
		{
			return (ExpressionBranch)ParseExpressionRecursive(expression, operators);
		}
		static IExpressionNode ParseExpressionRecursive(string expression, string[][] operators)
		{
			if (!expression.Contains(' '))
			{
				return new ExpressionLeaf { Expression = expression };
			}

			Dictionary<int, string> topLevelParentheses;
			expression = AddImplicitParentheses(expression, operators, out topLevelParentheses);

			var left = topLevelParentheses.First();
			var right = topLevelParentheses.Last();
			return new ExpressionBranch
			{
				Left = ParseExpressionRecursive(left.Value, operators),
				Right = ParseExpressionRecursive(right.Value, operators),
				Operator = expression.Substring(left.Value.Length + 3, right.Key - left.Value.Length - 5),
				Expression = expression
			};
		}

		static string AddImplicitParentheses(string expression, string[][] operators, out Dictionary<int, string> topLevelParentheses)
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

				bool cont = false;
				foreach (string[] operatorGroup in operators)
				{
					KeyValuePair<string, string>[] split = maskedExpression.SplitWithSeparator(operatorGroup.Select(s => ' ' + s + ' '), StringComparison.CurrentCultureIgnoreCase).ToArray();
					for (int i = 0, index = 0; i < split.Length; i++)
					{
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

		public static T EvaluateTree<T>(ExpressionBranch tree, Dictionary<string, T> valueLookup, Func<T, T, string, T> operation)
		{
			Func<IExpressionNode, T> getOrRecurse = node => valueLookup.ContainsKey(node.Expression)
				? valueLookup[node.Expression] : EvaluateTree((ExpressionBranch)node, valueLookup, operation);
			return operation(getOrRecurse(tree.Left), getOrRecurse(tree.Right), tree.Operator);
		}
	}
}
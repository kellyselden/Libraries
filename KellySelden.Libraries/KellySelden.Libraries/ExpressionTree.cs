using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KellySelden.Libraries
{
	public class ExpressionTree
	{
		public class Hierarchy
		{
			public Hierarchy Left { get; set; }
			public Hierarchy Right { get; set; }
			public Operator Operator { get; set; }
			public string ExpressionSection { get; set; }

			public Hierarchy(string expressionSection)
			{
				ExpressionSection = expressionSection;
			}
		}
		public enum Operator
		{
			Or,
			And
		}

		static readonly Dictionary<string, Operator> OperatorStrings = new Dictionary<string, Operator>
		{
			{ "or", Operator.Or },
			{ "and", Operator.And }
		};

		public Hierarchy ParseFilterExpression(string expression)
		{
			if (!expression.Contains(' '))
			{
				return new Hierarchy(expression);
			}

			Dictionary<int, string> groups;
			while (true)
			{
				var leftIndexes = expression.IndexOfAll('(').ToDictionary(i => '(');
				var rightIndexes = expression.IndexOfAll(')').ToDictionary(i => ')');
				if (leftIndexes.Count != rightIndexes.Count)
				{
					throw new InvalidOperationException("filter expression parentheses do not match up");
				}
				if (leftIndexes.Count == 1 && leftIndexes.Single().Key == 0 && rightIndexes.Single().Key == expression.Length - 1)
				{
					expression = expression.Substring(1, expression.Length - 2);
					continue;
				}

				groups = new Dictionary<int, string>();
				string cleared = expression;
				int count = 0;
				int leftStartIndex = 0;
				foreach (KeyValuePair<int, char> kvp in leftIndexes.Union(rightIndexes).OrderBy(p => p.Key))
				{
					if (kvp.Value == ')' && count == 0)
					{
						throw new InvalidOperationException("filter expression parentheses do not match up");
					}
					if (kvp.Value == '(')
					{
						if (count == 0)
						{
							leftStartIndex = kvp.Key + 1;
						}
						count++;
					}
					else
					{
						count--;
						if (count == 0)
						{
							int length = kvp.Key - leftStartIndex;
							string substring = expression.Substring(leftStartIndex, length);
							groups.Add(leftStartIndex, substring);
							cleared = cleared.Replace(substring, "".PadRight(length, 'x'));
						}
					}
				}

				bool cont = false;
				foreach (string operatorString in OperatorStrings.Select(kvp => kvp.Key))
				{
					string splitString = ' ' + operatorString + ' ';
					string[] split = Regex.Split(cleared, splitString, RegexOptions.IgnoreCase);
					for (int i = 0, index = 0; i < split.Length; i++)
					{
						string s = split[i];
						if (s == cleared) break;
						int length = s.Length;
						foreach (KeyValuePair<int, string> kvp in groups)
						{
							if (kvp.Key >= index && kvp.Key < index + length)
							{
								s = s.Replace(s.Substring(kvp.Key - index, kvp.Value.Length), kvp.Value);
							}
						}
						if (s.First() != '(' || s.Last() != ')')
						{
							s = '(' + s + ')';
							cont = true;
						}
						if (split.Length > 2)
						{
							if (i == 0)
								s = "".PadLeft(split.Length - 2, '(') + s;
							else if (i != split.Length - 1)
								s += ')';
							cont = true;
						}
					index += split[i].Length + splitString.Length;
						split[i] = s;
					}
					if (cont)
					{
						expression = string.Join(splitString, split);
						break;
					}
				}
				if (!cont) break;
			}

			var parent = new Hierarchy(expression);
			parent.Left = ParseFilterExpression(groups.First().Value);
			parent.Right = ParseFilterExpression(groups.Last().Value);
			parent.Operator = OperatorStrings[expression.Substring(groups.First().Value.Length + 3, groups.Last().Key - groups.First().Value.Length - 5).ToLower()];
			return parent;
		}
	}
}
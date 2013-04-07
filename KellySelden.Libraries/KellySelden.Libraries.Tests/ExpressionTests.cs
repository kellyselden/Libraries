using System;
using System.Collections.Generic;
using System.Linq;
using KellySelden.Libraries.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests
{
	[TestClass]
	public class ExpressionTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var x = new Expression(StringComparison.CurrentCultureIgnoreCase).ParseExpression("group0 AND (group1 AND (group2 OR group3)) OR group4 AND group5", Sql);
			var y = new Expression(StringComparison.CurrentCultureIgnoreCase).ParseExpression("(group0 OR (group1 OR (group2 AND group3)) AND group4 OR group5) AND (group0 OR (group1 OR (group2 AND group3)) AND group4 OR group5)", Sql);
			var yy = new Expression(StringComparison.CurrentCultureIgnoreCase).EvaluateTree(y, new Dictionary<string, IEnumerable<int>>
			{
				{ "group0", new[] { 0, 3, 4 } },
				{ "group1", new[] { 0, 1, 2 } },
				{ "group2", new[] { 2, 3 } },
				{ "group3", new[] { 2, 3 } },
				{ "group4", new[] { 2, 3 } },
				{ "group5", new[] { 2, 3 } }
			}, (a, b, c) =>
			{
				switch (c)
				{
					case "OR":
						return a.Union(b);
					case "AND":
						return a.Intersect(b);
				}
				throw new Exception();
			});

			var z = new Expression(StringComparison.CurrentCultureIgnoreCase).EvaluateTree(new Expression(StringComparison.CurrentCultureIgnoreCase).ParseExpression("group0 and (Group1 OR group2)", Sql), new Dictionary<string, IEnumerable<int>>
			{
				{ "Group0", new[] { 0, 3, 4 } },
				{ "group1", new[] { 0, 1, 2 } },
				{ "group2", new[] { 2, 3 } }
			}, (a, b, c) =>
			{
				switch (c.ToUpper())
				{
					case "OR":
						return a.Union(b);
					case "AND":
						return a.Intersect(b);
				}
				throw new Exception();
			});

			var value = new Expression().EvaluateExpression("var0 + (var1 - var2 * var3) / var4", new[]
			{
				new[] { "+", "-" },
				new[] { "*", "/" }
			}, new Dictionary<string, decimal>
			{
				{ "var0", 78.6743M },
				{ "var1", 54.3454M },
				{ "var2", 523.34M },
				{ "var3", 7.43M },
				{ "var4", 34.903M }
			}, (a, b, c) =>
			{
				switch (c)
				{
					case "+":
						return a + b;
					case "-":
						return a - b;
					case "*":
						return a * b;
					case "/":
						return a / b;
				}
				throw new Exception();
			});
		}

		static readonly string[][] Sql = new[]
		{
			new[] { "OR" },
			new[] { "AND" }
		};
	}
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests
{
	[TestClass]
	public class ExpressionTreeTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var x = new ExpressionTree().ParseFilterExpression("group0 AND (group1 AND (group2 OR group3)) OR group4 AND group5");
			var y = new ExpressionTree().ParseFilterExpression("group0 OR (group1 OR (group2 AND group3)) AND group4 OR group5 AND group0 OR (group1 OR (group2 AND group3)) AND group4 OR group5");
		}
	}
}
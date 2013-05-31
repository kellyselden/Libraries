using System.Linq;
using KellySelden.Libraries.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests
{
	[TestClass]
	public class StringExtensionsTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var x = "hey,you".SplitWithSeparator(new[] { ',' }).ToArray();
		}
	}
}
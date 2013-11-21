using System.Linq;
using KellySelden.Libraries.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests
{
	[TestClass]
	public class EnumHelpersTests
	{
		enum Test { Test1 }

		[TestMethod]
		public void TestMethod1()
		{
			var actual = EnumHelpers.ToEnumerable<Test>();

			Assert.AreEqual(1, actual.Count());
			Assert.AreEqual(Test.Test1, actual.ElementAt(0));
		}
	}
}
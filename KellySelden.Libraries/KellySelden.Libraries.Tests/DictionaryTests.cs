using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KellySelden.Libraries.Extensions;

namespace KellySelden.Libraries.Tests
{
	[TestClass]
	public class DictionaryTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			IDictionary<int, string> x = new int[0].ToDictionaryValue(i => "");
		}
	}
}
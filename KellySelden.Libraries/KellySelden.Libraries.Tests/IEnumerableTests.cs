using System.Collections.Generic;
using System.Linq;
using KellySelden.Libraries.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests
{
	[TestClass]
	public class IEnumerableTests
	{
		[TestMethod]
		public void ToArrayFast()
		{
			var x = new int[0];
			Assert.AreSame(x, x.ToArrayFast());
			var y = (IEnumerable<int>)x;
			Assert.AreSame(y, y.ToArrayFast());
			var z = new List<int>();
			Assert.AreNotSame(z, z.ToArrayFast());
		}

		[TestMethod]
		public void GroupByCount_Empty()
		{
			var y = new int[0].GroupByCount(2);
			Assert.AreEqual(1, y.Count());
			Assert.AreEqual(0, y.ElementAt(0).Count());
		}

		[TestMethod]
		public void GroupByCount_HalfFilled()
		{
			var x = new List<int>();
			for (int i = 0; i < 3; i++)
			{
				x.Add(i);
			}
			var y = x.GroupByCount(2);
			Assert.AreEqual(2, y.Count());
			Assert.AreEqual(2, y.ElementAt(0).Count());
			Assert.AreEqual(1, y.ElementAt(1).Count());
			Assert.AreEqual(0, y.ElementAt(0).ElementAt(0));
			Assert.AreEqual(1, y.ElementAt(0).ElementAt(1));
			Assert.AreEqual(2, y.ElementAt(1).ElementAt(0));
		}

		[TestMethod]
		public void GroupByCount_Filled()
		{
			var x = new List<int>();
			for (int i = 0; i < 4; i++)
			{
				x.Add(i);
			}
			var y = x.GroupByCount(2);
			Assert.AreEqual(2, y.Count());
			Assert.AreEqual(2, y.ElementAt(0).Count());
			Assert.AreEqual(2, y.ElementAt(1).Count());
			Assert.AreEqual(0, y.ElementAt(0).ElementAt(0));
			Assert.AreEqual(1, y.ElementAt(0).ElementAt(1));
			Assert.AreEqual(2, y.ElementAt(1).ElementAt(0));
			Assert.AreEqual(3, y.ElementAt(1).ElementAt(1));
		}
	}
}
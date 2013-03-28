using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests
{
	[TestClass]
	public class ParallelRecursionTests
	{
		Random _random;

		void Reseed()
		{
			_random = new Random(1);
		}

		class Node
		{
			public List<Node> Children { get; private set; }
			public int Value { get; set; }
			public string NodeString { get; private set; }

			public Node(string parentString, int node)
			{
				Children = new List<Node>();
				NodeString = string.Format("{0}node: {1}", parentString == "" ? "" : parentString + ", ", node);
			}

			public void Print()
			{
				Debug.WriteLine("Thread: {0}, {1}, value: {2}", Thread.CurrentThread.ManagedThreadId, NodeString, Value);
			}
		}

		void BuildTree(Node node, int level, int nodes)
		{
			if (level == 0)
			{
				node.Value = _random.Next(0, 100);
				return;
			}
			for (int i = 0; i < nodes; i++)
			{
				var child = new Node(node.NodeString, i);
				BuildTree(child, level - 1, nodes);
				node.Children.Add(child);
			}
		}

		Node _node1, _node2;

		[TestInitialize]
		public void TestInitialize()
		{
			Action<Node> buildTree = node => BuildTree(node, 3, 3);

			_node1 = new Node("", 0);
			Reseed();
			buildTree(_node1);

			_node2 = new Node("", 0);
			Reseed();
			buildTree(_node2);
		}

		[TestMethod]
		public void TestMethod1()
		{
			Action<Node> recursion = null;
			recursion = parent =>
			{
				foreach (Node child in parent.Children)
				{
					recursion(child);
					Thread.Sleep(10);
					parent.Value += child.Value;
					//node1.Value += child.Value;
					//child.Print();
				}
			};
			var stopwatch = Stopwatch.StartNew();
			recursion(_node1);
			_node1.Print();
			stopwatch.Stop();
			Debug.WriteLine(stopwatch.ElapsedMilliseconds);

			stopwatch.Restart();
			new ParallelRecursion(4).Start(_node2, (parent, recurse) =>
			{
				foreach (Node child in parent.Children)
				{
					recurse(child, c =>
					{
						Thread.Sleep(10);
						parent.Value += c.Value;
						//node2.Value += c.Value;
						//c.Print();
					});
				}
			});
			_node2.Print();
			stopwatch.Stop();
			Debug.WriteLine(stopwatch.ElapsedMilliseconds);

			Assert.AreEqual(_node1.Value, _node2.Value);
		}

		//[TestMethod]
		//public void TestMethod2()
		//{
		//	Func<Node, int> recursion = null;
		//	recursion = parent =>
		//	{
		//		int value = parent.Value;
		//		foreach (Node child in parent.Children)
		//		{
		//			value += recursion(child);
		//			Thread.Sleep(10);
		//		}
		//		return value;
		//	};
		//	var stopwatch = Stopwatch.StartNew();
		//	_node1.Value = recursion(_node1);
		//	stopwatch.Stop();
		//	Debug.WriteLine(stopwatch.ElapsedMilliseconds);

		//	stopwatch.Restart();
		//	_node2.Value = new ParallelRecursion(4).Start2(_node2, (parent, recurse) =>
		//	{
		//		int value = parent.Value;
		//		foreach (Node child in parent.Children)
		//		{
		//			value += recurse(child, c =>
		//			{
		//				Thread.Sleep(10);
		//				parent.Value += c.Value;
		//				//node2.Value += c.Value;
		//				//c.Print();
		//			});
		//		}
		//		return value;
		//	});
		//	stopwatch.Stop();
		//	Debug.WriteLine(stopwatch.ElapsedMilliseconds);

		//	Assert.AreEqual(_node1.Value, _node2.Value);
		//}
	}
}
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
		readonly ParallelRecursion _parallelRecursion = new ParallelRecursion(4);

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
				node.Value = 50;//_random.Next(0, 100);
				return;
			}
			for (int i = 0; i < nodes; i++)
			{
				var child = new Node(node.NodeString, i);
				BuildTree(child, level - 1, nodes);
				node.Children.Add(child);
			}
		}

		Node GetNode(Node node, string nodeString)
		{
			if (node.NodeString == nodeString)
			{
				return node;
			}
			foreach (Node child in node.Children)
			{
				Node value = GetNode(child, nodeString);
				if (value != null)
				{
					return value;
				}
			}
			return null;
		}

		Node _node1, _node2;

		[TestInitialize]
		public void TestInitialize()
		{
			Action<Node> buildTree = node => BuildTree(node, 4, 4);

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
					//Thread.Sleep(10);
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
			_parallelRecursion.Start(_node2, (parent, recurse) =>
			{
				foreach (Node child in parent.Children)
				{
					recurse(child, () =>
					{
						Thread.Sleep(10);
						parent.Value += child.Value;
						//node2.Value += c.Value;
						//c.Print();

						Node otherNode = GetNode(_node1, child.NodeString);
						if (otherNode.Value != child.Value)
						{
							int i = 0;
							otherNode.Print();
							child.Print();
						}
					});
				}
			});
			_node2.Print();
			stopwatch.Stop();
			Debug.WriteLine(stopwatch.ElapsedMilliseconds);

			Assert.AreEqual(_node1.Value, _node2.Value);
		}

		[TestMethod]
		public void TestMethod2()
		{
			Func<Node, int> recursion = null;
			recursion = parent =>
			{
				foreach (Node child in parent.Children)
				{
					parent.Value += recursion(child);
					//Thread.Sleep(10);
				}
				return parent.Value;
			};
			var stopwatch = Stopwatch.StartNew();
			_node1.Value = recursion(_node1);
			stopwatch.Stop();
			Debug.WriteLine(stopwatch.ElapsedMilliseconds);

			stopwatch.Restart();
			_node2.Value = _parallelRecursion.Start<Node, int>(_node2, (parent, recurse) =>
			{
				foreach (Node child in parent.Children)
				{
					recurse(child, v =>
					{
						parent.Value += v;
						Thread.Sleep(10);
					});
				}
				return () =>
				{
					Node otherNode = GetNode(_node1, parent.NodeString);
					if (otherNode.Value != parent.Value)
					{
						int i = 0;
						otherNode.Print();
						parent.Print();
					}
					return parent.Value;
				};
			});
			stopwatch.Stop();
			Debug.WriteLine(stopwatch.ElapsedMilliseconds);

			Assert.AreEqual(_node1.Value, _node2.Value);
		}

		[TestMethod]
		public void TestMethod3()
		{
			Func<Node, int> recursion = null;
			recursion = parent =>
			{
				foreach (Node child in parent.Children)
				{
					//Thread.Sleep(10);
					parent.Value += recursion(child);
				}
				return parent.Value;
			};
			var stopwatch = Stopwatch.StartNew();
			_node1.Value = recursion(_node1);
			stopwatch.Stop();
			Debug.WriteLine(stopwatch.ElapsedMilliseconds);

			int threadId = Thread.CurrentThread.ManagedThreadId;

			stopwatch.Restart();
			Func<Node, int> func = _parallelRecursion.Start<Node, int>((parent, recurse) =>
			{
				if (Thread.CurrentThread.ManagedThreadId != threadId)
					Thread.Sleep(10);
				foreach (Node child in parent.Children)
				{
					recurse(child, v =>
					{
						parent.Value += v;
					});
				}
				return () =>
				{
					Node otherNode = GetNode(_node1, parent.NodeString);
					if (otherNode.Value != parent.Value)
					{
						int i = 0;
						otherNode.Print();
						parent.Print();
					}
					return parent.Value;
				};
			});
			_node2.Value = func(_node2);
			stopwatch.Stop();
			Debug.WriteLine(stopwatch.ElapsedMilliseconds);

			Assert.AreEqual(_node1.Value, _node2.Value);
		}
	}
}
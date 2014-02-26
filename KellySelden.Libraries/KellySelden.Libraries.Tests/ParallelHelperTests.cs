using System;
using System.Threading;
using System.Threading.Tasks;
using KellySelden.Libraries.Parallel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KellySelden.Libraries.Tests
{
	[TestClass]
	public class ParallelHelperTests
	{
		[TestMethod]
		public void ParallelHelper_StartNewTask_UseTaskTrue()
		{
			int x = 0;
			Action action = () => Interlocked.Increment(ref x);

			Task task1 = ParallelHelper.StartNewTask(true, action);
			Task task2 = ParallelHelper.StartNewTask(true, action);
			task1.Wait();
			task2.Wait();

			Assert.AreEqual(2, x);
			Assert.AreNotEqual(task1, task2);
		}

		[TestMethod]
		public void ParallelHelper_StartNewTask_UseTaskFalse()
		{
			int x = 0;
			Action action = () => Interlocked.Increment(ref x);

			Task task1 = ParallelHelper.StartNewTask(false, action);
			Task task2 = ParallelHelper.StartNewTask(false, action);
			task1.Wait();
			task2.Wait();

			Assert.AreEqual(2, x);
			Assert.AreEqual(task1, task2);
		}

		[TestMethod]
		public void ParallelHelper_StartNewTaskGeneric_UseTaskTrue()
		{
			int x = 0;
			Func<int> func = () => Interlocked.Increment(ref x);

			Task task1 = ParallelHelper.StartNewTask(true, func);
			Task task2 = ParallelHelper.StartNewTask(true, func);
			task1.Wait();
			task2.Wait();

			Assert.AreEqual(2, x);
			Assert.AreNotEqual(task1, task2);
		}

		[TestMethod]
		public void ParallelHelper_StartNewTaskGeneric_UseTaskFalse()
		{
			int x = 0;
			Func<int> func = () => Interlocked.Increment(ref x);

			Task task1 = ParallelHelper.StartNewTask(false, func);
			Task task2 = ParallelHelper.StartNewTask(false, func);
			task1.Wait();
			task2.Wait();

			Assert.AreEqual(2, x);
			Assert.AreNotEqual(task1, task2);
		}
	}
}
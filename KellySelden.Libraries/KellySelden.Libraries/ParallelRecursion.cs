using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace KellySelden.Libraries
{
	public class ParallelRecursion
	{
		readonly int _numberOfTasks;

		public ParallelRecursion(int numberOfThreads)
		{
			if (numberOfThreads < 1)
				throw new ArgumentOutOfRangeException("numberOfThreads", numberOfThreads, "must run on a positive number of threads");
			if (numberOfThreads > Environment.ProcessorCount)
				throw new ArgumentOutOfRangeException("numberOfThreads", numberOfThreads, "more threads than processors is inefficient");
			_numberOfTasks = numberOfThreads - 1;
		}

		public void Start<TInput>(TInput firstElement, Action<TInput, Action<TInput, Action<TInput>>> bodyFromCaller)
		{
			var taskWaitList = new ConcurrentDictionary<int, ConcurrentBag<Task>>();
			Action<TInput, Action<TInput>> recursiveWrapper = null;
			Func<Task[]> removeAndReturnTasks = () =>
			{
				Task[] tasks;
				int threadId = Thread.CurrentThread.ManagedThreadId;
				lock (taskWaitList)
				{
					if (taskWaitList.ContainsKey(threadId))
					{
						ConcurrentBag<Task> tasks2;
						taskWaitList.TryRemove(threadId, out tasks2);
						tasks = tasks2.ToArray();
					}
					else
					{
						tasks = new Task[0];
					}
				}
				return tasks;
			};
			recursiveWrapper = (nextElement, callback) =>
			{
				Action action = () =>
				{
					bodyFromCaller(nextElement, recursiveWrapper);

					Task.WaitAll(removeAndReturnTasks());

					callback(nextElement);
				};
				bool runSynchronously = false;
				lock (taskWaitList)
				{
					if (taskWaitList.Count < _numberOfTasks)
					{
						int threadId = Thread.CurrentThread.ManagedThreadId;
						if (!taskWaitList.ContainsKey(threadId))
							taskWaitList.TryAdd(threadId, new ConcurrentBag<Task>());

						taskWaitList[threadId].Add(Task.Factory.StartNew(action));
					}
					else
					{
						runSynchronously = true;
					}
				}
				if (runSynchronously)
					action();
			};
			bodyFromCaller(firstElement, recursiveWrapper);

			Task.WaitAll(removeAndReturnTasks());
		}

		//public TOutput Start2<TInput, TOutput>(TInput firstElement, Action<TInput, Action<TInput, Action<TInput>>> bodyFromCaller)
		//{
		//	var taskWaitList = new ConcurrentDictionary<int, ConcurrentBag<Task>>();
		//	Action<TInput, Action<TInput>> recursiveWrapper = null;
		//	Func<Task[]> removeAndReturnTasks = () =>
		//	{
		//		Task[] tasks;
		//		int threadId = Thread.CurrentThread.ManagedThreadId;
		//		lock (taskWaitList)
		//		{
		//			if (taskWaitList.ContainsKey(threadId))
		//			{
		//				ConcurrentBag<Task> tasks2;
		//				taskWaitList.TryRemove(threadId, out tasks2);
		//				tasks = tasks2.ToArray();
		//			}
		//			else
		//			{
		//				tasks = new Task[0];
		//			}
		//		}
		//		return tasks;
		//	};
		//	recursiveWrapper = (nextElement, callback) =>
		//	{
		//		Action action = () =>
		//		{
		//			bodyFromCaller(nextElement, recursiveWrapper);

		//			Task.WaitAll(removeAndReturnTasks());

		//			callback(nextElement);
		//		};
		//		bool runSynchronously = false;
		//		lock (taskWaitList)
		//		{
		//			if (taskWaitList.Count < _numberOfTasks)
		//			{
		//				int threadId = Thread.CurrentThread.ManagedThreadId;
		//				if (!taskWaitList.ContainsKey(threadId))
		//					taskWaitList.TryAdd(threadId, new ConcurrentBag<Task>());

		//				taskWaitList[threadId].Add(Task.Factory.StartNew(action));
		//			}
		//			else
		//			{
		//				runSynchronously = true;
		//			}
		//		}
		//		if (runSynchronously)
		//			action();
		//	};
		//	bodyFromCaller(firstElement, recursiveWrapper);

		//	Task.WaitAll(removeAndReturnTasks());
		//}
	}
}
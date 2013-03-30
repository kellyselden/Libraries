using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace KellySelden.Libraries
{
	public class ParallelRecursion
	{
		readonly int _numberOfTasks;
		ConcurrentDictionary<int, ConcurrentBag<Task>> taskWaitList = new ConcurrentDictionary<int, ConcurrentBag<Task>>();

		public ParallelRecursion(int numberOfThreads)
		{
			if (numberOfThreads < 1)
				throw new ArgumentOutOfRangeException("numberOfThreads", numberOfThreads, "must run on a positive number of threads");
			if (numberOfThreads > Environment.ProcessorCount)
				throw new ArgumentOutOfRangeException("numberOfThreads", numberOfThreads, "more threads than processors is inefficient");
			_numberOfTasks = numberOfThreads - 1;
		}

		Task[] RemoveAndReturnTasks()
		{
			Task[] tasks;
			int threadId = Thread.CurrentThread.ManagedThreadId;
			lock (taskWaitList)
			{
				if (taskWaitList.ContainsKey(threadId))
				{
					ConcurrentBag<Task> taskList;
					taskWaitList.TryRemove(threadId, out taskList);
					tasks = taskList.ToArray();
				}
				else
				{
					tasks = new Task[0];
				}
			}
			return tasks;
		}

		void AddTask(Action action)
		{
			int threadId = Thread.CurrentThread.ManagedThreadId;
			if (!taskWaitList.ContainsKey(threadId))
				taskWaitList.TryAdd(threadId, new ConcurrentBag<Task>());

			taskWaitList[threadId].Add(Task.Factory.StartNew(action));
		}

		public void Start<TInput>(TInput firstElement, Action<TInput, Action<TInput, Action>> bodyFromCaller)
		{
			Start(bodyFromCaller)(firstElement);
		}

		public Action<TInput> Start<TInput>(Action<TInput, Action<TInput, Action>> bodyFromCaller)
		{
			Action<TInput, Action> recursiveWrapper = null;

			Action<TInput> recurseThenWait = element =>
			{
				bodyFromCaller(element, recursiveWrapper);

				Task[] tasks = RemoveAndReturnTasks();
				Task.WaitAll(tasks);
			};

			recursiveWrapper = (nextElement, callback) =>
			{
				Action recurse = () =>
				{
					recurseThenWait(nextElement);
					callback();
				};

				bool runSynchronously = false;
				lock (taskWaitList)
				{
					if (taskWaitList.Count < _numberOfTasks)
						AddTask(recurse);
					else runSynchronously = true;
				}
				if (runSynchronously) recurse();
			};

			return recurseThenWait;
		}

		//public Action<TInput> Start<TInput>(Action<TInput, Action<TInput, Action>> bodyFromCaller)
		//{
		//	return z => Start<TInput, object>((i, j) =>
		//	{
		//		bodyFromCaller(i, (x, y) => j(x, a => y()));
		//		return () => null;
		//	})(z);
		//}

		public TOutput Start<TInput, TOutput>(TInput firstElement, Func<TInput, Action<TInput, Action<TOutput>>, Func<TOutput>> bodyFromCaller)
		{
			return Start(bodyFromCaller)(firstElement);
		}

		public Func<TInput, TOutput> Start<TInput, TOutput>(Func<TInput, Action<TInput, Action<TOutput>>, Func<TOutput>> bodyFromCaller)
		{
			Action<TInput, Action<TOutput>> recursiveWrapper = null;

			Func<TInput, TOutput> recurseThenWaitThenReturnValue = element =>
			{
				Func<TOutput> returnValueGetter = bodyFromCaller(element, recursiveWrapper);

				Task[] tasks = RemoveAndReturnTasks();
				Task.WaitAll(tasks);

				return returnValueGetter();
			};

			recursiveWrapper = (nextElement, callback) =>
			{
				Action recurse = () => callback(recurseThenWaitThenReturnValue(nextElement));

				bool runSynchronously = false;
				lock (taskWaitList)
				{
					if (taskWaitList.Count < _numberOfTasks)
						AddTask(recurse);
					else runSynchronously = true;
				}
				if (runSynchronously) recurse();
			};

			return recurseThenWaitThenReturnValue;
		}

		//public Func<TInput, TOutput> Start<TInput, TOutput>(Func<TInput, Action<TInput, Action<TOutput>>, Func<TOutput>> bodyFromCaller)
		//{
		//	return StartCSharp4<TInput, TOutput>((i, j) =>
		//	{
		//		return bodyFromCaller(i, (x, y) =>
		//		{
		//			j(x, (a, b) =>
		//			{
		//				y(b);
		//			});
		//		});
		//	});
		//}

		public Func<TInput, TOutput> StartCSharp4<TInput, TOutput>(Func<TInput, Action<TInput, Action<TInput, TOutput>>, Func<TOutput>> bodyFromCaller)
		{
			Action<TInput, Action<TInput, TOutput>> recursiveWrapper = null;

			Func<TInput, TOutput> recurseThenWaitThenReturnValue = element =>
			{
				Func<TOutput> returnValueGetter = bodyFromCaller(element, recursiveWrapper);

				Task[] tasks = RemoveAndReturnTasks();
				Task.WaitAll(tasks);

				return returnValueGetter();
			};

			recursiveWrapper = (nextElement, callback) =>
			{
				Action recurse = () => callback(nextElement, recurseThenWaitThenReturnValue(nextElement));

				bool runSynchronously = false;
				lock (taskWaitList)
				{
					if (taskWaitList.Count < _numberOfTasks)
						AddTask(recurse);
					else runSynchronously = true;
				}
				if (runSynchronously) recurse();
			};

			return recurseThenWaitThenReturnValue;
		}
	}
}
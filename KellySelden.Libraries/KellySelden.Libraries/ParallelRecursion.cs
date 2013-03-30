using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace KellySelden.Libraries
{
	public class ParallelRecursion
	{
		readonly int _numberOfTasks;
		readonly ConcurrentDictionary<int, ConcurrentBag<Task>> _taskWaitList = new ConcurrentDictionary<int, ConcurrentBag<Task>>();

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
			lock (_taskWaitList)
			{
				if (_taskWaitList.ContainsKey(threadId))
				{
					ConcurrentBag<Task> taskList;
					_taskWaitList.TryRemove(threadId, out taskList);
					tasks = taskList.ToArray();
				}
				else tasks = new Task[0];
			}
			return tasks;
		}

		void RunTask(Action action)
		{
			bool runSynchronously = false;
			lock (_taskWaitList)
			{
				if (_taskWaitList.Count < _numberOfTasks)
				{
					int threadId = Thread.CurrentThread.ManagedThreadId;
					if (!_taskWaitList.ContainsKey(threadId))
						_taskWaitList.TryAdd(threadId, new ConcurrentBag<Task>());

					_taskWaitList[threadId].Add(Task.Factory.StartNew(action));
				}
				else runSynchronously = true;
			}
			if (runSynchronously) action();
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

				Task.WaitAll(RemoveAndReturnTasks());
			};

			recursiveWrapper = (nextElement, callback) => RunTask(() =>
			{
				recurseThenWait(nextElement);
				callback();
			});

			return recurseThenWait;
		}

		//public Action<TInput> Start<TInput>(Action<TInput, Action<TInput, Action>> bodyFromCaller)
		//{
		//	return StartCSharp4<TInput>((i, j) => bodyFromCaller(i, (x, y) => j(x, a => y())));
		//}

		public Action<TInput> StartCSharp4<TInput>(Action<TInput, Action<TInput, Action<TInput>>> bodyFromCaller)
		{
			Action<TInput, Action<TInput>> recursiveWrapper = null;

			Action<TInput> recurseThenWait = element =>
			{
				bodyFromCaller(element, recursiveWrapper);

				Task.WaitAll(RemoveAndReturnTasks());
			};

			recursiveWrapper = (nextElement, callback) => RunTask(() =>
			{
				recurseThenWait(nextElement);
				callback(nextElement);
			});

			return recurseThenWait;
		}

		//public Action<TInput> StartCSharp4<TInput>(Action<TInput, Action<TInput, Action<TInput>>> bodyFromCaller)
		//{
		//	return z => StartCSharp4<TInput, object>((i, j) =>
		//	{
		//		bodyFromCaller(i, (x, y) => j(x, (a, b) => y(a)));
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

				Task.WaitAll(RemoveAndReturnTasks());

				return returnValueGetter();
			};

			recursiveWrapper = (nextElement, callback) => RunTask(() =>
				callback(recurseThenWaitThenReturnValue(nextElement)));

			return recurseThenWaitThenReturnValue;
		}

		//public Func<TInput, TOutput> Start<TInput, TOutput>(Func<TInput, Action<TInput, Action<TOutput>>, Func<TOutput>> bodyFromCaller)
		//{
		//	return StartCSharp4<TInput, TOutput>((i, j) => bodyFromCaller(i, (x, y) => j(x, (a, b) => y(b))));
		//}

		public Func<TInput, TOutput> StartCSharp4<TInput, TOutput>(Func<TInput, Action<TInput, Action<TInput, TOutput>>, Func<TOutput>> bodyFromCaller)
		{
			Action<TInput, Action<TInput, TOutput>> recursiveWrapper = null;

			Func<TInput, TOutput> recurseThenWaitThenReturnValue = element =>
			{
				Func<TOutput> returnValueGetter = bodyFromCaller(element, recursiveWrapper);

				Task.WaitAll(RemoveAndReturnTasks());

				return returnValueGetter();
			};

			recursiveWrapper = (nextElement, callback) => RunTask(() =>
				callback(nextElement, recurseThenWaitThenReturnValue(nextElement)));

			return recurseThenWaitThenReturnValue;
		}
	}
}
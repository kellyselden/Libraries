using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KellySelden.Libraries.Parallel
{
	public static class ParallelHelper
	{
		public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, Action<TSource, Synchronize> body)
		{
			return ForEach(source, new ParallelOptions(), body);
		}
		public static ParallelLoopResult ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource, Synchronize> body)
		{
			var semaphore = new SemaphoreSlim(0);
			TSource[] array = source.ToArray();
			int count = array.Count() - 1;
			int i = 0;
			return System.Threading.Tasks.Parallel.ForEach(array, parallelOptions, item => body(item, () =>
			{
				lock (semaphore)
				{
					if (i == count)
					{
						if (i > 0)
						{
							Console.WriteLine("about to release first time");
							semaphore.Release();
						}
						return;
					}
					Interlocked.Increment(ref i);
				}
				Console.WriteLine("about to wait");
				semaphore.Wait();
				lock (semaphore)
				{
					Interlocked.Decrement(ref i);
					if (i > 0)
					{
						Console.WriteLine("about to release");
						semaphore.Release();
					}
				}
			}));
		}

		public delegate void Synchronize();
	}
}
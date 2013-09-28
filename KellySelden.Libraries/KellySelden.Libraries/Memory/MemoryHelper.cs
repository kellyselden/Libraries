using System;
using System.Diagnostics;
using System.Threading;

namespace KellySelden.Libraries.Memory
{
	public class MemoryHelper
	{
		readonly Semaphore _semaphore;
		readonly int? _thresholdInMb, _maximumInMb;

		Process _process;

		internal MemoryHelper(int? initialThreads, int? thresholdInMb, int? maximumInMb)
		{
			int threads = initialThreads ?? 1;
			_semaphore = new Semaphore(threads, threads);
			_thresholdInMb = thresholdInMb;
			_maximumInMb = maximumInMb;
		}

		public void PerformAction(Action<CheckForEarlyRelease> action)
		{
			bool released = false;
			_semaphore.WaitOne();
			try
			{
				action(() =>
				{
					if (released) return;

					if (_thresholdInMb.HasValue && PerformanceInfo.GetPhysicalAvailableMemoryInMiB() < _thresholdInMb)
						return;

					if (_maximumInMb.HasValue)
					{
						if (_process == null) _process = Process.GetCurrentProcess();
						if (_process.PrivateMemorySize64 / 1024 / 1024 > _maximumInMb)
							return;
					}

					_semaphore.Release(1);
					released = true;
				});
			}
			finally
			{
				if (!released) _semaphore.Release(1);
			}
		}

		public delegate void CheckForEarlyRelease();
	}

	public class MemoryHelperBuilder
	{
		int? _initialThreads, _thresholdInMb, _maximumInMb;

		public MemoryHelperBuilder SetInitialThreads(int initialThreads)
		{
			_initialThreads = initialThreads;
			return this;
		}

		public MemoryHelperBuilder SetThreshold(int thresholdInMb)
		{
			_thresholdInMb = thresholdInMb;
			return this;
		}

		public MemoryHelperBuilder SetMaximum(int maximumInMb)
		{
			_maximumInMb = maximumInMb;
			return this;
		}

		public MemoryHelper Build()
		{
			if (!_thresholdInMb.HasValue && !_maximumInMb.HasValue)
				throw new InvalidOperationException("must specify threshold and/or maximum");
			return new MemoryHelper(_initialThreads, _thresholdInMb, _maximumInMb);
		}
	}
}
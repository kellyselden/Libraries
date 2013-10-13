using System;

namespace KellySelden.Libraries.Mvc.Infrastructure
{
	public class GenericDisposable : IDisposable
	{
		readonly Action _disposeAction;

		public GenericDisposable(Action disposeAction)
		{
			_disposeAction = disposeAction;
		}

		public void Dispose()
		{
			_disposeAction();
		}
	}
}
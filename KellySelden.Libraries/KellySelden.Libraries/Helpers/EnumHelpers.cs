using System;
using System.Collections.Generic;
using System.Linq;

namespace KellySelden.Libraries.Helpers
{
	public static class EnumHelpers
	{
		public static IEnumerable<T> ToEnumerable<T>()
		{
			return from object e in Enum.GetValues(typeof(T)) select (T)e;
		}
	}
}
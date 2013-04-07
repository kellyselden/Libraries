using System;
using System.Collections.Generic;
using System.Linq;

namespace KellySelden.Libraries
{
	public static class DictionaryExtensions
	{
		public static IDictionary<TSource, TElement> ToDictionary<TSource, TElement>(this IEnumerable<TSource> source,
			Func<TSource, TElement> valueSelector)
		{
			return source.ToDictionary(x => x, valueSelector);
		}
	}
}
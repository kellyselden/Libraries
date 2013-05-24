using System;
using System.Collections.Generic;
using System.Linq;

namespace KellySelden.Libraries.Extensions
{
	public static class DictionaryExtensions
	{
		public static IDictionary<TSource, TElement> ToDictionaryValue<TSource, TElement>(this IEnumerable<TSource> source,
			Func<TSource, TElement> valueSelector)
		{
			return source.ToDictionary(x => x, valueSelector);
		}

		public static IDictionary<TKey, TElement> Union<TKey, TElement>(this IDictionary<TKey, TElement> first,
			IDictionary<TKey, TElement> second)
		{
			return Enumerable.Union(first, second).ToDictionary(g => g.Key, g => g.Value);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace KellySelden.Libraries.Extensions
{
	public static class DictionaryExtensions
	{
		public static Dictionary<TSource, TElement> ToDictionaryValue<TSource, TElement>(this IEnumerable<TSource> source,
			Func<TSource, TElement> valueSelector)
		{
			return source.ToDictionary(x => x, valueSelector);
		}

		public static Dictionary<TKey, TElement> Union<TKey, TElement>(this IDictionary<TKey, TElement> first,
			IDictionary<TKey, TElement> second)
		{
			return Enumerable.Union(first, second).ToDictionary(g => g.Key, g => g.Value);
		}

		public static Dictionary<TKey, TNewValue> ToNewValue<TKey, TValue, TNewValue>(this IDictionary<TKey, TValue> source,
			Func<TValue, TNewValue> valueSelector)
		{
			return source.ToDictionary(kvp => kvp.Key, kvp => valueSelector(kvp.Value));
		}

		public static Dictionary<TKey, Dictionary<TValueKey, TElement>> ValueToDictionary<TKey, TValueKey, TElement>(
			this IDictionary<TKey, IEnumerable<TElement>> source,
			Func<TElement, TValueKey> valueKeySelector)
		{
			return source.ToNewValue(value => value.ToDictionary(valueKeySelector));
		}

		public static Dictionary<TKey, Dictionary<TValueKey, IEnumerable<TElement>>> ToGroupedValue<TKey, TValueKey, TElement>(
			this IDictionary<TKey, IEnumerable<TElement>> source,
			Func<TElement, TValueKey> valueKeySelector)
		{
			return source.ToNewValue(value => value.GroupBy(valueKeySelector).ToDictionary(g => g.Key, g => g.AsEnumerable()));
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace KellySelden.Libraries.Extensions
{
	public static class IEnumerableExtensions
	{
		public static T[] ToArrayFast<T>(this IEnumerable<T> collection)
		{
			return collection as T[] ?? collection.ToArray();
		}

		public static IEnumerable<IEnumerable<T>> GroupByCount<T>(this IEnumerable<T> collection, int count)
		{
			var list = new List<T>();
			T[] array = collection.ToArrayFast();
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(array[i]);
				if ((i + 1) % count == 0)
				{
					yield return list;
					list.Clear();
				}
			}
			if (array.Length == 0 || list.Count != 0)
				yield return list;
		}

		public static TResult SelectFirst<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			return selector(source.First());
		}
	}
}
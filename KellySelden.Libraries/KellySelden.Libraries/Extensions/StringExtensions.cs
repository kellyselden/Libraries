using System;
using System.Collections.Generic;
using System.Linq;

namespace KellySelden.Libraries.Extensions
{
	public static class StringExtensions
	{
		public static IEnumerable<int> IndexOfAll(this string str, char value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			return str.IndexOfAll(value.ToString(), comparisonType);
		}
		public static IEnumerable<int> IndexOfAll(this string str, string value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			for (int index = 0; (index = str.IndexOf(value, index, comparisonType)) != -1; index += value.Length)
				yield return index;
		}

		public static IEnumerable<string> Split(this string str, IEnumerable<char> separator,
			StringComparison comparisonType = StringComparison.CurrentCulture, StringSplitOptions options = StringSplitOptions.None)
		{
			return str.Split(separator.Select(c => c.ToString()), comparisonType, options);
		}
		public static IEnumerable<string> Split(this string str, IEnumerable<string> separator,
			StringComparison comparisonType = StringComparison.CurrentCulture, StringSplitOptions options = StringSplitOptions.None)
		{
			return str.SplitWithSeparator(separator, comparisonType, options).Select(kvp => kvp.Key);
		}
		public static IEnumerable<KeyValuePair<string, char>> SplitWithSeparator(this string str, IEnumerable<char> separator,
			StringComparison comparisonType = StringComparison.CurrentCulture, StringSplitOptions options = StringSplitOptions.None)
		{
			return str.SplitWithSeparator(separator.Select(c => c.ToString()), comparisonType, options).Select(kvp => new KeyValuePair<string, char>(kvp.Key, kvp.Value[0]));
		}
		public static IEnumerable<KeyValuePair<string, string>> SplitWithSeparator(this string str, IEnumerable<string> separator,
			StringComparison comparisonType = StringComparison.CurrentCulture, StringSplitOptions options = StringSplitOptions.None)
		{
			IEnumerable<KeyValuePair<int, string>> indexes = new Dictionary<int, string>();
			indexes = separator.Aggregate(indexes, (current, s) => current.Union(DictionaryExtensions.ToDictionary(str.IndexOfAll(s, comparisonType), i => s)));
			int lastIndex = 0;
			var list = new List<KeyValuePair<string, string>>();
			foreach (KeyValuePair<int, string> kvp in indexes.OrderBy(a => a.Key))
			{
				string substring = str.Substring(lastIndex, kvp.Key - lastIndex);
				list.Add(new KeyValuePair<string, string>(substring, kvp.Value));
				lastIndex += substring.Length + kvp.Value.Length;
			}
			list.Add(new KeyValuePair<string, string>(str.Substring(lastIndex), null));
			return list.Where(a =>
			{
				switch (options)
				{
					case StringSplitOptions.None:
						return true;
					case StringSplitOptions.RemoveEmptyEntries:
						return a.Key != "";
					default:
						throw new NotSupportedException("unrecognized StringSplitOptions");
				}
			});
		}
	}
}
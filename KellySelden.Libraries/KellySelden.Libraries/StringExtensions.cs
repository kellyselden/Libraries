using System;
using System.Collections.Generic;

namespace KellySelden.Libraries
{
	public static class StringExtensions
	{
		public static IEnumerable<int> IndexOfAll(this string str, char value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			return IndexOfAll(str, value.ToString(), comparisonType);
		}

		public static IEnumerable<int> IndexOfAll(this string str, string value, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			for (int index = 0; (index = str.IndexOf(value, index, comparisonType)) != -1; index += value.Length)
				yield return index;
		}
	}
}
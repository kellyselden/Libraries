using System;
using System.Collections.Generic;
using System.Linq;

namespace KellySelden.Libraries.Sql.Math
{
	public static class SqlDecimalExtensions
	{
		public static SqlDecimalAny Average(this IEnumerable<SqlDecimalBase> source)
		{
			var s = source.ToArray();
			return s.Sum() / s.Length;
		}
		public static SqlDecimalAny Average<T>(this IEnumerable<T> source, Func<T, SqlDecimalBase> selector)
		{
			return Average(source.Select(selector));
		}

		public static SqlDecimalAny Sum(this IEnumerable<SqlDecimalBase> source)
		{
			SqlDecimalAny sum = SqlDecimalBase.Null;
			foreach (SqlDecimalBase d in source)
			{
				if (!SqlDecimalBase.IsNull(d))
				{
					if (sum == SqlDecimalBase.Null) sum = 0;
					sum += d;
				}
			}
			return sum;
		}
		public static SqlDecimalAny Sum<T>(this IEnumerable<T> source, Func<T, SqlDecimalBase> selector)
		{
			return Sum(source.Select(selector));
		}
	}
}
using System;

namespace KellySelden.Libraries.Extensions
{
	public static class DateTimeExtensions
	{
		public static bool IsBetween(this DateTime now, DateTime start, DateTime end)
		{
			return start <= now && now < end;
		}
	}
}
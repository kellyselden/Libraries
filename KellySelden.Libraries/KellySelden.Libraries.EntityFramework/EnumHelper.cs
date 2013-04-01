using System;
using System.Linq;

namespace KellySelden.Libraries.EntityFramework
{
	public static class EnumHelper
	{
		public static string GetDisplay(this Enum @enum)
		{
			string value = @enum.ToString();
			EnumAttribute attribute = @enum.GetType().GetField(value).GetCustomAttributes(false)
				.Cast<Attribute>().SingleOrDefault(p => p is EnumAttribute) as EnumAttribute;
			return attribute != null ? attribute.Display : value;
		}
	}

	public class EnumAttribute : Attribute
	{
		public string Display { get; set; }

		public EnumAttribute(string display)
		{
			Display = display;
		}
	}
}
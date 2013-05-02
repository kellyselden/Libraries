using System.Collections.Generic;
using System.Web;

namespace KellySelden.Libraries.Mvc.Infrastructure
{
	static class RequestCache
	{
		public static HashSet<string> Includes
		{
			get
			{
				var includes = HttpContext.Current.Items["Includes"] as HashSet<string>;
				if (includes == null) HttpContext.Current.Items["Includes"] = includes = new HashSet<string>();
				return includes;
			}
		}
	}
}
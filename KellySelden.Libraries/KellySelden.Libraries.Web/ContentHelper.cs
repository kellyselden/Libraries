using System;
using System.IO;
using System.Web;

namespace KellySelden.Libraries.Web
{
	public static class ContentHelper
	{
		public static IHtmlString RenderScript(string url)
		{
			return Render("<script src=\"{0}\" type=\"text/javascript\"></script>", url);
		}

		public static IHtmlString RenderStyle(string url)
		{
			return Render("<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />", url);
		}

		public static IHtmlString Render(string format, string url)
		{
			return new HtmlString(String.Format(format, url));
		}

		public static void AddInclude(IHtmlString s)
		{
			RequestCache.Includes.Add(s.ToString());
		}

		public static IHtmlString RenderIncludes()
		{
			return new HtmlString(String.Join("\n", RequestCache.Includes));
		}

		public static string AppendTimestampQuery(string path)
		{
			string physicalPath = HttpContext.Current.Request.MapPath(path);
			if (!File.Exists(physicalPath))
				throw new FileNotFoundException("file not found", path);

			var info = new FileInfo(physicalPath);
			return String.Format("{0}?{1}{2}", path, info.LastWriteTimeUtc.Ticks % 1000, info.Length % 1000);
		}
	}
}
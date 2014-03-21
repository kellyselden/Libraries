using System.Web;
using System.Web.UI;
using KellySelden.Libraries.Web;

namespace KellySelden.Libraries.WebForms
{
	public static class Helper
	{
		public static void QueueScript(Control control, string path)
		{
			ContentHelper.AddInclude(RenderScript(control, path));
		}

		public static void QueueStyle(Control control, string path)
		{
			ContentHelper.AddInclude(RenderStyle(control, path));
		}

		public static void QueueEmbeddedScript<T>(Control control, string path)
		{
			ContentHelper.AddInclude(RenderEmbeddedScript<T>(control, path));
		}

		public static void QueueEmbeddedStyle<T>(Control control, string path)
		{
			ContentHelper.AddInclude(RenderEmbeddedStyle<T>(control, path));
		}

		public static IHtmlString RenderScript(Control control, string path)
		{
			return ContentHelper.RenderScript(ContentHelper.AppendTimestampQuery(path));
		}

		public static IHtmlString RenderStyle(Control control, string path)
		{
			return ContentHelper.RenderStyle(ContentHelper.AppendTimestampQuery(path));
		}

		static IHtmlString RenderEmbeddedScript<T>(Control control, string path)
		{
			return ContentHelper.RenderScript(GetResourceUrl<T>(control, path));
		}

		static IHtmlString RenderEmbeddedStyle<T>(Control control, string path)
		{
			return ContentHelper.RenderStyle(GetResourceUrl<T>(control, path));
		}

		public static IHtmlString RenderIncludes()
		{
			return ContentHelper.RenderIncludes();
		}

		static string GetResourceUrl<T>(Control control, string path)
		{
			return string.Format("{0}&n={1}", control.Page.ClientScript.GetWebResourceUrl(typeof(T), path), path);
		}
	}
}
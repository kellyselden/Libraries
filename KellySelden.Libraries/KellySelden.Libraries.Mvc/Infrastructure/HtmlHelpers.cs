using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Handlers;
using System.Web.Mvc;
using System.Web.UI;

namespace KellySelden.Libraries.Mvc.Infrastructure
{
	public static class HtmlHelpers
	{
		public static IHtmlString ActionButton(this HtmlHelper html, string value, string action, string controller = null, object routeValues = null)
		{
			var button = new TagBuilder("input");
			button.Attributes["type"] = "button";
			button.Attributes["value"] = value;
			button.Attributes["onclick"] = string.Format("location.href='{0}'",
				new UrlHelper(html.ViewContext.RequestContext).Action(action, controller, routeValues));

			return new HtmlString(button.ToString(TagRenderMode.SelfClosing));
		}

		public static IHtmlString SubmitLink(this HtmlHelper html, string value)
		{
			var anchor = new TagBuilder("a");
			anchor.Attributes["href"] = "javascript:void(0)";
			anchor.Attributes["onclick"] = "$(this).closest('form')[0].submit()";
			anchor.InnerHtml = value;

			return new HtmlString(anchor.ToString());
		}

		public static void QueueScript(this HtmlHelper html, string path)
		{
			RequestCache.Includes.Add(RenderScript(html, path).ToString());
		}

		public static void QueueStyle(this HtmlHelper html, string path)
		{
			RequestCache.Includes.Add(RenderStyle(html, path).ToString());
		}

		public static void QueueEmbeddedScript(this HtmlHelper html, string path)
		{
			RequestCache.Includes.Add(RenderEmbeddedScript(html, path).ToString());
		}

		public static void QueueEmbeddedStyle(this HtmlHelper html, string path)
		{
			RequestCache.Includes.Add(RenderEmbeddedStyle(html, path).ToString());
		}

		public static IHtmlString RenderScript(this HtmlHelper html, string path)
		{
			return RenderScript(AppendQuery(path));
		}

		public static IHtmlString RenderStyle(this HtmlHelper html, string path)
		{
			return RenderStyle(AppendQuery(path));
		}

		static IHtmlString RenderEmbeddedScript(HtmlHelper html, string path)
		{
			return RenderScript(GetWebResourceUrl(html, path));
		}

		static IHtmlString RenderEmbeddedStyle(HtmlHelper html, string path)
		{
			return RenderStyle(GetWebResourceUrl(html, path));
		}

		static IHtmlString RenderScript(string url)
		{
			return Render("<script src=\"{0}\" type=\"text/javascript\"></script>", url);
		}

		static IHtmlString RenderStyle(string url)
		{
			return Render("<link href=\"{0}\" rel=\"stylesheet\" type=\"text/css\" />", url);
		}

		static IHtmlString Render(string format, string url)
		{
			return new HtmlString(string.Format(format, url));
		}

		static readonly Type Me = typeof(HtmlHelpers);
		static readonly string Namespace = Me.Assembly.FullName.Substring(0, Me.Assembly.FullName.IndexOf(','));
		static MethodInfo GetWebResourceUrlMethod = typeof(AssemblyResourceLoader).GetMethod(
			"GetWebResourceUrl",
			BindingFlags.NonPublic | BindingFlags.Static, null,
			new[] { typeof(Type), typeof(string) }, null);

		public static string GetWebResourceUrl(this HtmlHelper html, string path)
		{
			return (string)GetWebResourceUrlMethod.Invoke(null, new object[] { Me, Namespace + path.Replace("/", ".") });
		}

		public static MvcHtmlString RenderIncludes(this HtmlHelper html)
		{
			return MvcHtmlString.Create(string.Join("\n", RequestCache.Includes));
		}

		static string AppendQuery(string path)
		{
			string physicalPath = HttpContext.Current.Request.MapPath(path);
			if (!File.Exists(physicalPath))
				throw new FileNotFoundException("file not found", path);

			var info = new FileInfo(physicalPath);
			return string.Format("{0}?{1}{2}", path, info.LastWriteTimeUtc.Ticks % 1000, info.Length % 1000);
		}
	}
}
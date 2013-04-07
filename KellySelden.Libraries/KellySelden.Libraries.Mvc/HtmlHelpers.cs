using System.Web;
using System.Web.Mvc;

namespace KellySelden.Libraries.Mvc
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
	}
}
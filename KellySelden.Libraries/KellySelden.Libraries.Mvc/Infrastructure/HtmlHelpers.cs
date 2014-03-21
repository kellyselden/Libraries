using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Handlers;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.WebPages;
using KellySelden.Libraries.Web;

namespace KellySelden.Libraries.Mvc.Infrastructure
{
	public static class HtmlHelpers
	{
		public static MvcHtmlString FileUpload(this HtmlHelper html, string name, string value)
		{
			var file = new TagBuilder("input");
			file.Attributes["type"] = "file";
			file.Attributes["name"] = value;
			file.Attributes["onchange"] = "$(this).next().click()";

			var submit = new TagBuilder("input");
			submit.Attributes["type"] = "submit";
			submit.Attributes["name"] = name;
			submit.Attributes["value"] = value;
			submit.Attributes["style"] = "display:none";

			return MvcHtmlString.Create(file.ToString(TagRenderMode.SelfClosing) + submit.ToString(TagRenderMode.SelfClosing));
		}

		public static MvcHtmlString Popup(this HtmlHelper html, string show, string hide, Func<object, HelperResult> template, object htmlAttributes = null)
		{
			html.QueueEmbeddedScript(KellySeldenLinks.Scripts.Popup_js);
			html.QueueEmbeddedStyle(KellySeldenLinks.Content.Popup_css);

			string id = Guid.NewGuid().ToString();

			var child = new TagBuilder("div");
			child.Attributes["id"] = id;
			child.AddCssClass("Popup");
			child.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
			child.InnerHtml = template(null).ToString();

			var script = new TagBuilder("script");
			script.Attributes["type"] = "text/javascript";
			script.InnerHtml = string.Format("Popup_init($('#{0}'), '{1}', '{2}');", id, show, hide);

			var parent = new TagBuilder("div");
			parent.InnerHtml = child.ToString() + script.ToString();

			return MvcHtmlString.Create(parent.ToString());
		}

		public static MvcHtmlString DisplayRow(this HtmlHelper html, string expression)
		{
			return CreateRow(html, expression,
				"<span>" + html.Display(expression) + "</span>");
		}

		public static MvcHtmlString DisplayRowFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
		{
			return CreateRow(html, expression,
				"<span>" + html.DisplayFor(expression) + "</span>");
		}

		public static MvcHtmlString EditorRow(this HtmlHelper html, string expression)
		{
			return CreateRow(html, expression,
				html.Editor(expression).ToString() +
				html.ValidationMessage(expression).ToString());
		}

		public static MvcHtmlString EditorRowFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
		{
			return CreateRow(html, expression,
				html.EditorFor(expression).ToString() +
				html.ValidationMessageFor(expression).ToString());
		}

		static MvcHtmlString CreateRow(HtmlHelper html, string expression, string content)
		{
			return CreateRow(ExpressionHelper.GetExpressionText(expression), html.Label(expression) + content);
		}
		static MvcHtmlString CreateRow<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string content)
		{
			return CreateRow(ExpressionHelper.GetExpressionText(expression), html.LabelFor(expression) + content);
		}
		static MvcHtmlString CreateRow(string id, string content)
		{
			return MvcHtmlString.Create("<div id=\"" + TagBuilder.CreateSanitizedId(id) + "Row\">" + content + "</div>");
		}

		public static IHtmlString Markup(this HtmlHelper html, Func<object, IHtmlString> func)
		{
			return func(null);
		}

		public static IHtmlString ActionButton(this HtmlHelper html, string value, string action, string controller = null, object routeValues = null)
		{
			var button = new TagBuilder("input");
			button.Attributes["type"] = "button";
			button.Attributes["value"] = value;
			button.Attributes["onclick"] = string.Format("location.href='{0}'",
				new UrlHelper(html.ViewContext.RequestContext).Action(action, controller, routeValues));

			return new HtmlString(button.ToString(TagRenderMode.SelfClosing));
		}

		public static IHtmlString SubmitLink(this HtmlHelper html, string linkText)
		{
			var anchor = new TagBuilder("a");
			anchor.Attributes["href"] = "javascript:void(0)";
			anchor.Attributes["onclick"] = "$(this).closest('form')[0].submit()";
			anchor.InnerHtml = linkText;

			return new HtmlString(anchor.ToString());
		}

		public static IHtmlString Link(this HtmlHelper html, string linkText, string href, object htmlAttributes = null)
		{
			var anchor = new TagBuilder("a");
			anchor.Attributes["href"] = href;
			anchor.InnerHtml = linkText;
			anchor.MergeAttributes(HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));

			return new HtmlString(anchor.ToString());
		}

		public static GenericDisposable CenteredContainer(this HtmlHelper html)
		{
			return CenteredContainer(html, null);
		}
		public static GenericDisposable CenteredContainer(this HtmlHelper html, object htmlAttributes)
		{
			return CenteredContainer(html, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
		}
		public static GenericDisposable CenteredContainer(this HtmlHelper html, IDictionary<string, object> htmlAttributes)
		{
			QueueEmbeddedStyle(html, KellySeldenLinks.Content.CenteredContainer_css);

			var writer = html.ViewContext.Writer;

			var outer = new TagBuilder("div");
			outer.Attributes.Add("class", "CenteredContainer-outer");
			writer.Write(outer.ToString(TagRenderMode.StartTag));

			var inner = new TagBuilder("div");
			inner.MergeAttributes(htmlAttributes);
			if (inner.Attributes.ContainsKey("class"))
				inner.Attributes["class"] += " ";
			else
				inner.Attributes.Add("class", "");
			inner.Attributes["class"] += "CenteredContainer-inner";
			writer.Write(inner.ToString(TagRenderMode.StartTag));

			return new GenericDisposable(() =>
			{
				writer.Write(outer.ToString(TagRenderMode.EndTag));
				writer.Write(inner.ToString(TagRenderMode.EndTag));
			});
		}

		public static void QueueScript(this HtmlHelper html, string path)
		{
			ContentHelper.AddInclude(RenderScript(html, path));
		}

		public static void QueueStyle(this HtmlHelper html, string path)
		{
			ContentHelper.AddInclude(RenderStyle(html, path));
		}

		public static void QueueEmbeddedScript(this HtmlHelper html, string path)
		{
			ContentHelper.AddInclude(RenderEmbeddedScript(html, path));
		}

		public static void QueueEmbeddedStyle(this HtmlHelper html, string path)
		{
			ContentHelper.AddInclude(RenderEmbeddedStyle(html, path));
		}

		public static IHtmlString RenderScript(this HtmlHelper html, string path)
		{
			return ContentHelper.RenderScript(ContentHelper.AppendTimestampQuery(path));
		}

		public static IHtmlString RenderStyle(this HtmlHelper html, string path)
		{
			return ContentHelper.RenderStyle(ContentHelper.AppendTimestampQuery(path));
		}

		static IHtmlString RenderEmbeddedScript(HtmlHelper html, string path)
		{
			return ContentHelper.RenderScript(GetResourceUrl(html, path, "text/javascript"));
		}

		static IHtmlString RenderEmbeddedStyle(HtmlHelper html, string path)
		{
			return ContentHelper.RenderStyle(GetResourceUrl(html, path, "text/css"));
		}

		static string GetResourceUrl(HtmlHelper html, string path, string contentType)
		{
			//T4MVC hack: it prepends the virtual directory even though I'm not using it.
			var virtualDirectory = HttpRuntime.AppDomainAppVirtualPath;
			if (virtualDirectory != null && path.StartsWith(virtualDirectory + '/'))
				path = path.Remove(0, virtualDirectory.Length);

			return new UrlHelper(html.ViewContext.RequestContext)
				.Action(KellySeldenMVC.EmbeddedResource.Index(path, contentType));
		}

		static readonly Type Me = typeof(HtmlHelpers);
		static readonly string Namespace = Me.Assembly.FullName.Substring(0, Me.Assembly.FullName.IndexOf(','));
		static readonly MethodInfo GetWebResourceUrlMethod = typeof(AssemblyResourceLoader).GetMethod(
			"GetWebResourceUrl",
			BindingFlags.NonPublic | BindingFlags.Static, null,
			new[] { typeof(Type), typeof(string) }, null);

		public static string GetWebResourceUrl(this HtmlHelper html, string path)
		{
			return (string)GetWebResourceUrlMethod.Invoke(null, new object[] { Me, Namespace + path.Replace("/", ".") });
		}

		public static IHtmlString RenderIncludes(this HtmlHelper html)
		{
			return ContentHelper.RenderIncludes();
		}
	}
}
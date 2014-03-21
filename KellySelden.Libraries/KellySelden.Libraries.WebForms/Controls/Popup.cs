using System;
using System.Web.UI;

[assembly: WebResource("KellySelden.Libraries.WebForms.Styles.jquery.ui.button.min.css", "text/css")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Styles.jquery.ui.core.min.css", "text/css")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Styles.jquery.ui.dialog.min.css", "text/css")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Styles.jquery.ui.resizable.min.css", "text/css")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Styles.jquery.ui.theme.min.css", "text/css")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.jquery-1.11.0.min.js", "text/javascript")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.jquery.ui.button.min.js", "text/javascript")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.jquery.ui.core.min.js", "text/javascript")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.jquery.ui.dialog.min.js", "text/javascript")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.jquery.ui.draggable.min.js", "text/javascript")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.jquery.ui.mouse.min.js", "text/javascript")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.jquery.ui.position.min.js", "text/javascript")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.jquery.ui.resizable.min.js", "text/javascript")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.jquery.ui.widget.min.js", "text/javascript")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Images.ui-bg_highlight-soft_75_cccccc_1x100.png", "image/png")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Images.ui-icons_454545_256x240.png", "image/png")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Images.ui-icons_888888_256x240.png", "image/png")]
[assembly: WebResource("KellySelden.Libraries.WebForms.Styles.Popup.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("KellySelden.Libraries.WebForms.Scripts.Popup.js", "text/javascript")]
namespace KellySelden.Libraries.WebForms.Controls
{
	public class Popup : Control
	{
		static Type _type;
		static Type Type { get { return _type ?? (_type = typeof(Popup)); } }

		public string Title { get; set; }
		public bool OpenOnLoad { get; set; }
		public bool Modal { get; set; }
		public bool Draggable { get; set; }
		public bool Resizable { get; set; }
		public string OpenTriggerId { get; set; }
		public string CloseTriggerId { get; set; }

		protected bool LoadJQuery { get; set; }
		protected bool LoadJQueryUi { get; set; }
		protected bool LoadImages { get; set; }
		protected string OnCreateFunction { get; set; }

		public Popup()
		{
			LoadJQuery = true;
			LoadJQueryUi = true;
			LoadImages = true;
			Draggable = true;
			Resizable = true;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (LoadJQuery) Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.jquery-1.11.0.min.js");
			if (LoadJQueryUi)
			{
				Helper.QueueEmbeddedStyle<Popup>(this, "KellySelden.Libraries.WebForms.Styles.jquery.ui.button.min.css");
				Helper.QueueEmbeddedStyle<Popup>(this, "KellySelden.Libraries.WebForms.Styles.jquery.ui.core.min.css");
				Helper.QueueEmbeddedStyle<Popup>(this, "KellySelden.Libraries.WebForms.Styles.jquery.ui.dialog.min.css");
				Helper.QueueEmbeddedStyle<Popup>(this, "KellySelden.Libraries.WebForms.Styles.jquery.ui.theme.min.css");
				Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.jquery.ui.core.min.js");
				Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.jquery.ui.widget.min.js");
				Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.jquery.ui.mouse.min.js");
				Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.jquery.ui.position.min.js");
				Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.jquery.ui.button.min.js");
				Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.jquery.ui.dialog.min.js");
				if (Draggable)
					Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.jquery.ui.draggable.min.js");
				if (Resizable)
				{
					Helper.QueueEmbeddedStyle<Popup>(this, "KellySelden.Libraries.WebForms.Styles.jquery.ui.resizable.min.css");
					Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.jquery.ui.resizable.min.js");
				}
			}
			if (LoadImages) Helper.QueueEmbeddedStyle<Popup>(this, "KellySelden.Libraries.WebForms.Styles.Popup.css");
			Helper.QueueEmbeddedScript<Popup>(this, "KellySelden.Libraries.WebForms.Scripts.Popup.js");

			Page.ClientScript.RegisterClientScriptBlock(Type, ClientID,
				string.Format(@"
$(function() {{
	Popup_init('{0}', {{
		appendTo: 'form:first',
		autoOpen: {1},
		modal: {2},
		draggable: {3},
		resizable: {4},
		create: {5}
	}}, '{6}', '{7}');
}});",
					ClientID,
					OpenOnLoad.ToString().ToLower(),
					Modal.ToString().ToLower(),
					Draggable.ToString().ToLower(),
					Resizable.ToString().ToLower(),
					string.IsNullOrEmpty(OnCreateFunction) ? "function() { }" : OnCreateFunction,
					OpenTriggerId,
					CloseTriggerId), true);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
			writer.AddAttribute(HtmlTextWriterAttribute.Title, Title);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			RenderChildren(writer);
			writer.RenderEndTag();
		}

		public void AjaxOpen()
		{
			ScriptManager.RegisterClientScriptBlock(Page, Type, ClientID, string.Format("Popup_open('{0}');", ClientID), true);
		}
	}
}
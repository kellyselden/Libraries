using System.Web.Mvc;
using System.Web.Routing;
using KellySelden.Libraries.Mvc.Controllers;

namespace KellySelden.Libraries.Mvc
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes, string controller = EmbeddedResourceController.NameConst)
		{
			routes.MapRoute(
				controller,
				controller,
				new
				{
					controller = KellySeldenMVC.EmbeddedResource.Name,
					action = KellySeldenMVC.EmbeddedResource.ActionNames.Index
				});
		}
	}
}
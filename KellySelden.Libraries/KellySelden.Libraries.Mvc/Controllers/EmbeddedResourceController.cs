using System.Reflection;
using System.Web.Mvc;

namespace KellySelden.Libraries.Mvc.Controllers
{
	public partial class EmbeddedResourceController : Controller
	{
		static readonly Assembly Assembly = typeof(EmbeddedResourceController).Assembly;
		static readonly string Namespace = Assembly.FullName.Substring(0, Assembly.FullName.IndexOf(','));

		public virtual ActionResult Index(string path, string contentType)
		{
			return new FileStreamResult(
				Assembly.GetManifestResourceStream(Namespace + path.Replace("/", ".")),
				contentType);
		}
    }
}
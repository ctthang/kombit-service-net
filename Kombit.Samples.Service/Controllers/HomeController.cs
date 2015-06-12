#region

using System.Web.Mvc;

#endregion

namespace Kombit.Samples.Service.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
#region

using System.Web.Mvc;

#endregion

namespace Kombit.Samples.STS.Controllers
{
    /// <summary>
    ///     This class is to handle request to home page
    /// </summary>
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
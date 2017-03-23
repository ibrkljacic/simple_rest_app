using System.Web.Mvc;

namespace RZNU_SimpleRest.Controllers
{
    [Route("api")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
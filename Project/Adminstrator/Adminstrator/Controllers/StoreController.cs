using Microsoft.AspNetCore.Mvc;

namespace Adminstrator.Controllers
{
    public class StoreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

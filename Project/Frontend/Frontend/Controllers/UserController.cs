using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Profile()
        {

            return View();
        }
    }
}

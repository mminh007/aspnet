using Frontend.HttpsClient;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly AuthApiClient _authApi;

        public AuthenticationController(AuthApiClient authApi)
        {
            _authApi = authApi;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, token, message) = await _authApi.LoginAsync(model);

            if (!success || string.IsNullOrEmpty(token))
            {
                ViewBag.Error = message ?? "Login Failed!";
                return View(model);
            }

            HttpContext.Session.SetString("Token", token);
            HttpContext.Session.SetString("UserEmail", model.Email);

            TempData["Message"] = message ?? "Login Successfully!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Password dont match!";
                return View(model);
            }

            var (success, message) = await _authApi.RegisterAsync(model);

            if (!success)
            {
                ViewBag.Error = message ?? "Register Failed!";
                return View(model);
            }

            TempData["Message"] = message ?? "Register Sucessfully, Please Login!";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "Logout Successfully!";
            return RedirectToAction("Login");
        }
    }
}

using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Auths.Requests;
using Adminstrator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Adminstrator.Controllers
{
    public class AuthenticationController : Controller
    {
        
        private readonly IAuthService _authServices;

        public AuthenticationController( IAuthService authService)
        {
            _authServices = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, accessToken, refreshToken, expiresIn, role, userId, message, statusCode)
                = await _authServices.Login(model);

            if (!success || string.IsNullOrEmpty(accessToken))
            {
                SetErrorMessage(statusCode, $"{message} - {statusCode}", "Login");
                return View(model);
            }

            // add Access Token in Cookie
            Response.Cookies.Append("admin_accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddSeconds(expiresIn)
            });

            // add Refresh Token in Cookie
            if (!string.IsNullOrEmpty(refreshToken))
            {
                Response.Cookies.Append("admin_refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                });
            }

            // add user's info in Session
            HttpContext.Session.SetString("UserId", userId);
            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetString("UserRole", role);

            TempData["Message"] = message ?? "Login Successfully!";
            return RedirectToAction("Index", "Store");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Password don't match!";
                return View(model);
            }

            //model.Role = "seller"; // default role

            var (success, message, statusCode) = await _authServices.Register(model);

            if (!success)
            {
                SetErrorMessage(statusCode, message, "Register");
                return View(model);
            }

            TempData["Message"] = message;
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("admin_accessToken");
            Response.Cookies.Delete("admin_refreshToken");

            TempData["Message"] = "Logout Successfully!";
            return RedirectToAction("Login", "Authentication");
        }

        private void SetErrorMessage(int statusCode, string? message, string action)
        {
            ViewBag.Error = statusCode switch
            {
                400 => message ?? (action == "login" ? "Invalid email or password!" : "Bad request - Invalid input data!"),
                401 => "401 - Unauthorized - You don't have permission to access this resource.",
                409 => "409 - Conflict - Email address already exists in the system!",
                500 => "500 - Internal server error - Please try again later!",
                _ => message ?? $"{action} operation failed!"
            };
        }
    }
}

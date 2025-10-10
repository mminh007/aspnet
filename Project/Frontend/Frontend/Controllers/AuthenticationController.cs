using Frontend.HttpsClients.Auths;
using Frontend.Models.Auth.Requests;
using Frontend.Services;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthenticationController> _logger;
        public AuthenticationController(IAuthService authService, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, accessToken, refreshToken, expiresIn, role, userId, message, statusCode, verifyEmail)
               = await _authService.Login(model);

            if (verifyEmail == false)
            {
                TempData["Error"] = "Your email is not verified. Please check your inbox.";
                return RedirectToAction("VerifyEmail", new { email = model.Email });
            }

            if (!success || string.IsNullOrEmpty(accessToken))
            {
             
                SetErrorMessage(statusCode, $"{message} - {statusCode}", "Login");
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            _logger.LogInformation("Login - Token Accepted: {token}", accessToken);
            // add Access Token in Cookie
            Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddSeconds(expiresIn)
            });

            // add Refresh Token in Cookie
            if (!string.IsNullOrEmpty(refreshToken))
            {
                Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                });
            }

            

            // add user's info in Session
            HttpContext.Session.SetString("UserRole", role);
            HttpContext.Session.SetString("UserEmail", model.Email);
            HttpContext.Session.SetString("UserId", userId);

            TempData["Message"] = message ?? "Login Successfully!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl); 
            }

            return RedirectToAction("Index", "Home", new { id = userId });
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

            var (success, message, statusCode) = await _authService.Register(model);

            if (!success)
            {
                SetErrorMessage(statusCode, message, "Register");
                return View(model);
            }

            // ✅ Nếu đăng ký thành công, chuyển sang trang VerifyEmail
            TempData["RegisterMessage"] = message;
            TempData["RegisterEmail"] = model.EmailAddress;

            return RedirectToAction("VerifyEmail", new { email = model.EmailAddress });
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            ViewBag.Email = email;
            ViewBag.Message = TempData["RegisterMessage"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest model)
        {
            var (success, message, statusCode, data) = await _authService.VerifyEmail(model);

            if (success)
            {
                TempData["Message"] = "Email verified successfully! Please login.";
                return RedirectToAction("Login");
            }

            // ✅ Nếu hết hạn hoặc sai mã
            ViewBag.Error = message ?? "Invalid or expired verification code.";
            ViewBag.Email = model.Email;
            ViewBag.CanResend = true; // để View hiện nút resend
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResendCode(ResendCodeRequest model)
        {
            var (success, message, statusCode, data) = await _authService.ResendCode(model);

            if (!success)
            {
                ViewBag.Error = message ?? "Failed to resend code.";
            }
            else
            {
                ViewBag.Message = "A new code has been sent to your email.";
            }

            ViewBag.Email = model.Email;
            ViewBag.CanResend = false;
            return View("VerifyEmail");
        }

        public IActionResult Logout(string? returnUrl = null)
        {
            // Clear session
            HttpContext.Session.Clear();

            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            TempData["Message"] = "Logout Successfully!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var hasCookie = Request.Cookies.ContainsKey("accessToken");
            _logger.LogInformation("Before delete, accessToken exists in request? {HasCookie}", hasCookie);

            return RedirectToAction("Login", "Authentication");
        }


        private void SetErrorMessage(int statusCode, string? message, string action)
        {
            ViewBag.Error = statusCode switch
            {
                400 => message ??  (action == "login" ? "Invalid email or password!" : "Bad request - Invalid input data!"),
                401 => "401 - Unauthorized - You don't have permission to access this resource.",
                409 => "409 - Conflict - Email address already exists in the system!",
                500 => "500 - Internal server error - Please try again later!",
                _ => message ?? $"{action} operation failed!"
            };
        }
    }
}

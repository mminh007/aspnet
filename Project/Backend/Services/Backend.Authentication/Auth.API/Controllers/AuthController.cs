using Auth.BLL.External;
using Auth.BLL.Services.Interfaces;
using Auth.Common.Enums;
using Auth.Common.Models.Requests;
using Auth.Common.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenManager _tokenManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserApiService userApiService,
            IAuthService authService,
            ITokenManager tokenManager,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenManager = tokenManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while registering user: {@Model}", model);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Starting registration for {Email}", model.Email);

            var result = await _authService.RegisterWithUserApiAsync(model);
            return HandleResponse(result, "Register successfully!", "Registration failed");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while logging in: {@Model}", model);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Login attempt for {Email}", model.Email);

            var result = await _authService.Authenticate(model);
            return HandleResponse(result, "Login successfully!", "Invalid login credentials");
        }

        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest model)
        {
            _logger.LogInformation("Email verification attempt for {Email}", model.Email);
            var result = await _authService.VerifyEmailAsync(model.Email, model.Code);
            return HandleResponse(result, "Email verified successfully!", "Email verification failed");
        }

        [AllowAnonymous]
        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] ResendCodeRequest model)
        {
            _logger.LogInformation("Resend verification code request for {Email}", model.Email);

            var result = await _authService.ResendVerificationCodeAsync(model.Email);
            return HandleResponse(result, "Verification code resent successfully!", "Failed to resend verification code");
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel model)
        {
            _logger.LogInformation("Reset Password for {Email} - {Token} - {newPassword}", model.Email, model.VerifyCode, model.NewPassword);
            var result = await _authService.ResetPasswordAsync(model.Email, model.VerifyCode, model.NewPassword);
            return HandleResponse(result, "Password reset successfully!", "Invalid or expired token");
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/{email}")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            _logger.LogInformation("Forgot password request for {Email}", email);
            var result = await _authService.SendForgotPasswordEmailAsync(email);
            return HandleResponse(result, "Password reset email sent successfully!", "Failed to send password reset email");
        }


        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var rawRefreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(rawRefreshToken))
            {
                return BadRequest(new { statusCode = 400, message = "Refresh token is required" });
            }

            _logger.LogInformation("Refresh token attempt");

            var result = await _tokenManager.ValidateRefreshTokenAsync(rawRefreshToken);
            return HandleResponse(result, "Token refreshed successfully!", "Invalid or expired refresh token");
        }

        [AllowAnonymous]
        [HttpGet("system-token")]
        public IActionResult GetTokenSystem()
        {
            var result = _authService.GetTokenSystemAsync();

            return HandleResponse(result);
        }


         //✅ Method dùng chung để trả response
        private IActionResult HandleResponse<T>(AuthResponseModel<T> response, string? successMessage = null, string? failedMessage = null)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(new
                {
                    statusCode = 200,
                    message = successMessage ?? "Operation successful",
                    data = response.Data
                }),

                OperationResult.Failed => BadRequest(new
                {
                    statusCode = 400,
                    message = failedMessage ?? response.ErrorMessage ?? "Operation failed",
                    data = response.Data
                }),

                OperationResult.Forbidden => StatusCode(403, new
                {
                    statusCode = 403,
                    message = response.ErrorMessage ?? "Forbidden - You don't have permission",
                    data = response.Data
                }),

                OperationResult.NotFound => NotFound(new
                {
                    statusCode = 404,
                    message = response.ErrorMessage ?? "Not found",
                    data = response.Data
                }),

                OperationResult.Conflict => Conflict(new
                {
                    statusCode = 409,
                    message = response.ErrorMessage ?? "Conflict occurred",
                    data = response.Data
                }),

                OperationResult.Error => StatusCode(500, new
                {
                    statusCode = 500,
                    message = response.ErrorMessage ?? "Unexpected error!",
                    data = response.Data
                }),

                _ => StatusCode(500, new { statusCode = 500, message = "Unexpected error!" })
            };
        }
    }
}

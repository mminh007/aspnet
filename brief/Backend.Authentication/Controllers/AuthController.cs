using Backend.Authentication.Enums;
using Backend.Authentication.Models.Requests;
using Backend.Authentication.Models.Responses;
using Backend.Authentication.Services;
using Backend.Authentication.Services.External;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backend.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserApiService _userApiService;
        private readonly IAuthService _authService;
        private readonly ITokenManager _tokenManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserApiService userApiService,
            IAuthService authService,
            ITokenManager tokenManager,
            ILogger<AuthController> logger)
        {
            _userApiService = userApiService;
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

        // ✅ Method dùng chung để trả response
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
                    message = failedMessage ?? response.ErrorMessage ?? "Operation failed"
                }),

                OperationResult.NotFound => NotFound(new
                {
                    statusCode = 404,
                    message = response.ErrorMessage ?? "Not found"
                }),

                OperationResult.Error => StatusCode(500, new
                {
                    statusCode = 500,
                    message = response.ErrorMessage ?? "Unexpected error!"
                }),

                _ => StatusCode(500, new { statusCode = 500, message = "Unexpected error!" })
            };
        }
    }
}

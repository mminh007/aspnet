using Backend.Authentication.Enums;
using Backend.Authentication.HttpsClient;
using Backend.Authentication.Models;
using Backend.Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace Backend.Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserApiService _userApiService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserApiService userApiService,
            ITokenService tokenService,
            ILogger<AuthController> logger)
        {
            _userApiService = userApiService;
            _tokenService = tokenService;
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

            var userCheck = new CheckUserModel
            {
                Email = model.Email,
                Role = model.Role,
            };

            // Generate internal token for service-to-service call

            var response = await _userApiService.RegisterUserAsync(userCheck);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var items = await response.Content.ReadFromJsonAsync<RegisterUserResponseModel>();
                var userId = items!.UserId;

                var result = await _tokenService.RegisterAsync(model, userId);

                return HandleResponse(result, "Register successfully!", "Registration failed");
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
                return Conflict(new { message = "Email already exists!" });

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(new { message = "Invalid request to user service" });

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new { message = "User service unavailable" });

            return StatusCode(500, new { message = "Unexpected error!" });
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

            var result = await _tokenService.Authenticate(model);
            return HandleResponse(result, "Login successfully!", "Invalid login credentials");
        }

        // ✅ Method dùng chung để trả response
        private IActionResult HandleResponse(LoginResponseModel response, string? successMessage = null, string? failedMessage = null)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(new
                {
                    message = successMessage ?? "Operation successful",
                    access_token = response.AccessToken,
                    refresh_token = response.RefreshToken?.RawToken,
                    expires_in = response.ExpiresIn
                }),

                OperationResult.Failed => BadRequest(new
                {
                    message = failedMessage ?? response.ErrorMessage ?? "Operation failed"
                }),

                OperationResult.NotFound => NotFound(new
                {
                    message = response.ErrorMessage ?? "Not found"
                }),

                OperationResult.Error => StatusCode(500, new
                {
                    message = response.ErrorMessage ?? "Unexpected error!"
                }),

                _ => StatusCode(500, new { message = "Unexpected error!" })
            };
        }
    }
}

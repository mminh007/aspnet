using User.Common.Models.Requests;
using User.Common.Models.Responses;
using User.Common.Enums;
using User.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace User.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        [Authorize(Roles = "system")]
        public async Task<IActionResult> Register([FromBody] UserCheckModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while registering user: {@Model}", model);
                return BadRequest(new { statusCode = 400, message = "Invalid input data" });
            }

            var response = await _userService.RegisterAsync(model);
            return HandleResponse(response, "Register successfully!");
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStoreByUserID(Guid id)
        {
            var response = await _userService.GetStoreIdByUserId(id);
            return HandleResponse(response, "Get storeId successfully!");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var response = await _userService.GetUserAsync(userId);
            return HandleResponse(response, "Get user successfully!");
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UserUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while updating user: {@Model}", model);
                return BadRequest(new { statusCode = 400, message = "Invalid input data" });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var response = await _userService.UpdateUserAsync(model, userId);
            return HandleResponse(response, "Update user successfully!");
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var response = await _userService.DeleteUserAsync(userId);
            return HandleResponse(response, "Delete user successfully!", "User delete failed");
        }

       
        private IActionResult HandleResponse<T>(UserApiResponse<T> response, string? successMessage = null, string? failedMessage = null)
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

                OperationResult.Forbidden => StatusCode(403, new
                {
                    statusCode = 403,
                    message = response.ErrorMessage ?? "Forbidden - You don't have permission"
                }),

                OperationResult.NotFound => NotFound(new
                {
                    statusCode = 404,
                    message = response.ErrorMessage ?? "Not found"
                }),

                OperationResult.Conflict => Conflict(new
                {
                    statusCode = 409,
                    message = response.ErrorMessage ?? "Conflict occurred"
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

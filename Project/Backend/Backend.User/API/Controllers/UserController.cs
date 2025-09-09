using Commons.Models.Requests;
using Commons.Models.Responses;
using Commons.Enums;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Backend.User.Controllers
{
    [Route("api/[controller]")]
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
                return BadRequest(new UserApiResponse<string>
                {
                    Message = OperationResult.Failed,
                    ErrorMessage = "Invalid input data"
                });
            }

            var response = await _userService.RegisterAsync(model);
            return HandleResponse(response);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var response = await _userService.GetUserAsync(userId);
            return HandleResponse(response);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UserUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while updating user: {@Model}", model);
                return BadRequest(new UserApiResponse<string>
                {
                    Message = OperationResult.Failed,
                    ErrorMessage = "Invalid input data"
                });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var response = await _userService.UpdateUserAsync(model, userId);
            return HandleResponse(response);
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var response = await _userService.DeleteUserAsync(userId);
            return HandleResponse(response);
        }

        /// <summary>
        /// Helper để map OperationResult sang HTTP Status Code
        /// </summary>
        private IActionResult HandleResponse<T>(UserApiResponse<T> response)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(response),
                OperationResult.Failed => BadRequest(response),
                OperationResult.NotFound => NotFound(response),
                OperationResult.Conflict => Conflict(response),
                OperationResult.Error => StatusCode(500, response),
                _ => StatusCode(500, response)
            };
        }
    }
}

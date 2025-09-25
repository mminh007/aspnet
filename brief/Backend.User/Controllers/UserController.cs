using Backend.User.Models.Requests;
using Backend.User.Models.Responses;
using Backend.User.Services;
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
                    StatusCode = 400,
                    Message = "Invalid input data"
                });
            }

            var response = await _userService.RegisterAsync(model);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var response = await _userService.GetUserAsync(userId);
            return StatusCode(response.StatusCode, response);
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
                    StatusCode = 400,
                    Message = "Invalid input data"
                });
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var response = await _userService.UpdateUserAsync(model, userId);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var response = await _userService.DeleteUserAsync(userId);
            return StatusCode(response.StatusCode, response);
        }
    }
}

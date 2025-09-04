using Backend.User.Enums;
using Backend.User.Models;
using Backend.User.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpPost("register")]
        [Authorize(Roles = "system")]
        public async Task<IActionResult> Register([FromBody] UserCheckModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userRepository.CreateUserAsync(model);

            return result.Message switch
            {
                OperationResult.Success => Ok(new
                {
                    message = "Register successfully!",
                    userId = result.UserId
                }),

                OperationResult.Failed => Conflict(new
                {
                    message = result.ErrorMessage ?? "Email already exists!"
                }),

                OperationResult.NotFound => NotFound(new
                {
                    message = result.ErrorMessage ?? "User service unavailable"
                }),

                OperationResult.Error => StatusCode(500, new
                {
                    message = result.ErrorMessage ?? "Unexpected error!"
                }),

                _ => StatusCode(500, new { message = "Unexpected error!" })
            };
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var UserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            model.UserId = UserId;

            var result = await _userRepository.UpdateUserAsync(model);

            return result.Message switch
            {
                OperationResult.Success => Ok(new
                {
                    message = "User updated successfully!"
                }),

                OperationResult.NotFound => NotFound(new
                {
                    message = result.ErrorMessage ?? "User not found"
                }),

                OperationResult.Error => StatusCode(500, new
                {
                    message = result.ErrorMessage ?? "Unexpected error!"
                }),

                _ => StatusCode(500, new { message = "Unexpected error!" })
            };
        }

        [Authorize]
        [HttpDelete("delete/{userId:guid}")]
        public async Task<IActionResult> Delete()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var result = await _userRepository.DeleteUserAsync(userId);

            return result.Message switch
            {
                OperationResult.Success => Ok(new
                {
                    message = "User deleted successfully!"
                }),

                OperationResult.NotFound => NotFound(new
                {
                    message = result.ErrorMessage ?? "User not found"
                }),

                OperationResult.Error => StatusCode(500, new
                {
                    message = result.ErrorMessage ?? "Unexpected error!"
                }),

                _ => StatusCode(500, new { message = "Unexpected error!" })
            };
        }
    }
}

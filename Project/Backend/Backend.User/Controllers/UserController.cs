using Backend.User.Enums;
using Backend.User.HttpsClient;
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
        private readonly StoreApiService _storeApi;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger, StoreApiService storeApi)
        {
            _userRepository = userRepository;
            _logger = logger;
            _storeApi = storeApi;
        }

        [HttpPost("register")]
        [Authorize(Roles = "system")]
        public async Task<IActionResult> Register([FromBody] UserCheckModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while registering user: {@Model}", model);
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Registering new user with email {Email}", model.Email);

            var result = await _userRepository.CreateUserAsync(model);

            if (result.Message == OperationResult.Success)
            {
                var requestStore = new RegisterStoreModel()
                {
                    UserId = result.UserId,
                };

                _logger.LogInformation("User {UserId} created successfully. Attempting to create store...", result.UserId);

                var storeResult = await _storeApi.RegisterStoreAsync(requestStore);

                if (!storeResult.Success)
                {
                    _logger.LogWarning(
                        "User {UserId} created but failed to register store. Error: {Error}",
                        result.UserId,
                        storeResult.ErrorMessage
                    );

                    return Ok(new
                    {
                        message = "User created successfully, but store creation failed. Please create store manually in admin panel.",
                        userId = result.UserId
                    });
                }

                _logger.LogInformation("Store created successfully for User {UserId}", result.UserId);
            }

            return HandleResponse(result, "Register successfully!", "Email already exists!");
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateUser model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while updating user: {@Model}", model);
                return BadRequest(ModelState);
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            model.UserId = userId;

            _logger.LogInformation("Updating user {UserId}", userId);

            var result = await _userRepository.UpdateUserAsync(model);

            return HandleResponse(result, "User updated successfully!");
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            _logger.LogInformation("Deleting user {UserId}", userId);

            var result = await _userRepository.DeleteUserAsync(userId);

            return HandleResponse(result, "User deleted successfully!");
        }

        // ✅ Handle response
        private IActionResult HandleResponse(UserResponseModel response, string? successMessage = null, string? failedMessage = null)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(new
                {
                    message = successMessage ?? "Operation successful",
                    userId = response.UserId
                }),

                OperationResult.Failed => Conflict(new
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

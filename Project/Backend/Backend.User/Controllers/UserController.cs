using Backend.User.HttpsClients;
using Backend.User.Models;
using Backend.User.Repository;
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
        private readonly IUserRepository _userRepository;
        private readonly IStoreApiClient _storeApi;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, ILogger<UserController> logger, StoreApiClient storeApi)
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
                return BadRequest(new UserApiResponse<string>
                {
                    StatusCode = 400,
                    Message = "Invalid input data"
                });
            }

            _logger.LogInformation("Registering new user with email {Email}", model.Email);

            var result = await _userRepository.CreateUserAsync(model);

            if (!result.Success)
            {
                return Conflict(new UserApiResponse<string>
                {
                    StatusCode = 409,
                    Message = result.ErrorMessage ?? "Email already exists"
                });
            }

            // tạo store sau khi user được tạo
            var storeResult = await _storeApi.RegisterStoreAsync(new RegisterStoreModel { UserId = result.UserId });

            if (storeResult.StatusCode != 200)
            {
                return Ok(new UserApiResponse<object>
                {
                    StatusCode = 200,
                    Message = "User created successfully, but store creation failed. Please create store manually in admin panel.",
                    Data = new { userId = result.UserId }
                });
            }

            return Ok(new UserApiResponse<Guid>
            {
                StatusCode = 200,
                Message = "Register successfully!",
                Data = result.UserId
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            var userId = Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var result = await _userRepository.GetUserByIdAsync(userId);

            if (!result.Success)
            {
                return NotFound(new UserApiResponse<string>
                {
                    StatusCode = 404,
                    Message = result.ErrorMessage ?? "User not found"
                });
            }

            return Ok(new UserApiResponse<UserUpdateModel>
            {
                StatusCode = 200,
                Message = "Get user success",
                Data = result.UserInfo
            });
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

            model.UserId = userId;

            var result = await _userRepository.UpdateUserAsync(model);

            if (!result.Success)
            {
                return NotFound(new UserApiResponse<string>
                {
                    StatusCode = 404,
                    Message = result.ErrorMessage ?? "User not found"
                });
            }

            return Ok(new UserApiResponse<UserUpdateModel>
            {
                StatusCode = 200,
                Message = "User updated successfully!",
                Data = result.UserInfo
            });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var result = await _userRepository.DeleteUserAsync(userId);

            if (!result.Success)
            {
                return NotFound(new UserApiResponse<string>
                {
                    StatusCode = 404,
                    Message = result.ErrorMessage ?? "User not found"
                });
            }

            return Ok(new UserApiResponse<Guid>
            {
                StatusCode = 200,
                Message = "User deleted successfully!",
                Data = userId
            });
        }
    }
}

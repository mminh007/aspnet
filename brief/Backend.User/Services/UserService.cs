using Backend.User.Models.Requests;
using Backend.User.Models.Responses;
using Backend.User.Repository;
using Backend.User.Services.External;
using static Backend.User.Models.DTOs;

namespace Backend.User.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStoreApiClient _storeApi;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IStoreApiClient storeApi, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _storeApi = storeApi;
            _logger = logger;
        }

        public async Task<UserApiResponse<Guid>> RegisterAsync(UserCheckModel model)
        {
            var userId = await _userRepository.CreateUserAsync(model);

            if (userId == null)
            {
                return new UserApiResponse<Guid>
                {
                    StatusCode = 409,
                    Message = "Email already exists",
                    Data = Guid.Empty
                };
            }

            // gọi Store API sau khi user được tạo
            var storeResult = await _storeApi.RegisterStoreAsync(new RegisterStoreModel { UserId = userId.Value });

            if (storeResult.StatusCode != 200)
            {
                return new UserApiResponse<Guid>
                {
                    StatusCode = 200,
                    Message = "User created successfully, but store creation failed. Please create store manually in admin panel.",
                    Data = userId.Value
                };
            }

            return new UserApiResponse<Guid>
            {
                StatusCode = 200,
                Message = "Register successfully!",
                Data = userId.Value
            };
        }

        public async Task<UserApiResponse<UserDto>> GetUserAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return new UserApiResponse<UserDto>
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            return new UserApiResponse<UserDto>
            {
                StatusCode = 200,
                Message = "Get user success",
                Data = user
            };
        }

        public async Task<UserApiResponse<UserDto>> UpdateUserAsync(UserUpdateModel model, Guid userId)
        {
            model.UserId = userId;

            var updatedUser = await _userRepository.UpdateUserAsync(model);

            if (updatedUser == null)
            {
                return new UserApiResponse<UserDto>
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            return new UserApiResponse<UserDto>
            {
                StatusCode = 200,
                Message = "User updated successfully!",
                Data = updatedUser
            };
        }

        public async Task<UserApiResponse<Guid>> DeleteUserAsync(Guid userId)
        {
            var affectedRows = await _userRepository.DeleteUserAsync(userId);

            if (affectedRows == 0)
            {
                return new UserApiResponse<Guid>
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            return new UserApiResponse<Guid>
            {
                StatusCode = 200,
                Message = "User deleted successfully!",
                Data = userId
            };
        }
    }
}

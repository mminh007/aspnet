using User.BLL.External;
using User.Common.Enums;
using User.Common.Models.Requests;
using User.Common.Models.Responses;
using User.DAL.Repository.Interfaces;

using Microsoft.Extensions.Logging;
using static User.Common.Models.DTOs;

namespace User.BLL.Services
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

        public async Task<UserApiResponse<Guid?>> RegisterAsync(UserCheckModel model)
        {
            var existingUserId = await _userRepository.CheckUserAsync(model);
            if (existingUserId != Guid.Empty)
            {
                _logger.LogWarning("Registration failed - user already exists {UserId}", existingUserId);
                return new UserApiResponse<Guid?>
                {
                    Message = OperationResult.Failed,
                    ErrorMessage = "User already exists",
                    Data = existingUserId
                };
            }

            if (model.Role != "seller")
            {
                // Nếu không phải Seller thì chỉ tạo user, không tạo store
                var newUserId = await _userRepository.CreateUserAsync(model, null);
                _logger.LogInformation("User registered successfully {UserId}", newUserId);
                return new UserApiResponse<Guid?>
                {
                    Message = OperationResult.Success,
                    ErrorMessage = "User registered successfully",
                    Data = newUserId
                };
            }

            try
            {
                var newUserId = await _userRepository.CreateUserAsync(model, null);
                _logger.LogInformation("User created successfully {UserId}, attempting to create store", newUserId);

                // Tạo store cho Seller
                var storeResult = await _storeApi.RegisterStoreAsync(new RegisterStoreModel { UserId = newUserId.Value });

                if (storeResult.Message == OperationResult.Success)
                {
                    var storeId = storeResult.Data;
                    await _userRepository.UpdateUserAsync(new UserUpdateModel { UserId = newUserId.Value, StoreId = storeResult.Data });

                    _logger.LogInformation("Seller registered successfully with store {UserId}, {StoreId}", newUserId, storeId);
                    return new UserApiResponse<Guid?>
                    {
                        Message = OperationResult.Success,
                        ErrorMessage = "Seller registered successfully with store",
                        Data = newUserId
                    };
                }
                else
                {
                    _logger.LogWarning("Store registration failed for new user {UserId}: {ErrorMessage}", newUserId, storeResult.ErrorMessage);
                    return new UserApiResponse<Guid?>
                    {
                        Message = OperationResult.Failed,
                        ErrorMessage = "User created successfully but store creation failed. Please create store manually in admin panel.",
                        Data = newUserId
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while registering store for new user {UserId}");
                return new UserApiResponse<Guid?>
                {
                    Message = OperationResult.Failed,
                    ErrorMessage = "User created successfully but store creation failed due to an internal error. Please create store manually in admin panel.",
                };
            }
       
        }

        public async Task<UserApiResponse<UserDto>> GetUserAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return new UserApiResponse<UserDto>
                {
                    Message = OperationResult.NotFound,
                    ErrorMessage = "User not found"
                };
            }

            return new UserApiResponse<UserDto>
            {
                Message = OperationResult.Success,
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
                    Message = OperationResult.NotFound,
                    ErrorMessage = "User not found"
                };
            }

            return new UserApiResponse<UserDto>
            {
                Message = OperationResult.Success,
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
                    Message = OperationResult.NotFound,
                    ErrorMessage = "User not found"
                };
            }

            return new UserApiResponse<Guid>
            {
                Message = OperationResult.Success,
                Data = userId
            };
        }

        public async Task<UserApiResponse<List<Guid>>> GetStoreIdByUserId(Guid userId)
        {
            var listStores = await _userRepository.GetStoreIdsByUserIdAsync(userId);

            if (listStores == null || !listStores.Any())
            {
                return new UserApiResponse<List<Guid>>
                {
                    Message = OperationResult.NotFound,
                    ErrorMessage = "No stores found for the user"
                };
            }

            return new UserApiResponse<List<Guid>>
            {
                Message = OperationResult.Success,
                Data = listStores
            };
        }
    }
}
    

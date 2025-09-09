using BLL.External;
using BLL.Services;
using Commons.Enums;
using Commons.Models.Requests;
using Commons.Models.Responses;
using DAL.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using static Commons.Models.DTOs;

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
                Message = OperationResult.Conflict,
                ErrorMessage = "Email already exists",
                Data = Guid.Empty
            };
        }

        // gọi Store API sau khi user được tạo
        var storeResult = await _storeApi.RegisterStoreAsync(new RegisterStoreModel { UserId = userId.Value });

        if (storeResult.Message != OperationResult.Success)
        {
            return new UserApiResponse<Guid>
            {
                Message = OperationResult.Success, // user tạo thành công, store lỗi nhưng vẫn trả OK
                ErrorMessage = "User created successfully, but store creation failed. Please create store manually in admin panel.",
                Data = userId.Value
            };
        }

        return new UserApiResponse<Guid>
        {
            Message = OperationResult.Success,
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
}

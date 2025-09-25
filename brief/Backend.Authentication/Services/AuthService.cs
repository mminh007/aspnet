using Backend.Authentication.Enums;
using Backend.Authentication.Models.Entity;
using Backend.Authentication.Models.Requests;
using Backend.Authentication.Models.Responses;
using Backend.Authentication.Repository;
using Backend.Authentication.Services.External;
using Microsoft.AspNetCore.Identity;
using System.Net;
using static Backend.Authentication.Models.DTOs;

namespace Backend.Authentication.Services
{
    

    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;
        private readonly IPasswordHasher<IdentityModel> _passwordHasher;
        private readonly ITokenManager _tokenManager;
        private readonly UserApiService _userApiService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IAuthRepository repository,
                           IPasswordHasher<IdentityModel> passwordHasher,
                           ITokenManager tokenManager,
                           UserApiService userApiService,
                           ILogger<AuthService> logger)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _tokenManager = tokenManager;
            _userApiService = userApiService;
            _logger = logger;
        }

        public async Task<AuthResponseModel<string>> RegisterAsync(RegisterRequestModel request, Guid userId)
        {
            var newUser = new IdentityModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = request.Email,
                Role = request.Role,
                PasswordHashing = _passwordHasher.HashPassword(null!, request.Password)
            };

            try
            {
                await _repository.CreateIdentityAsync(newUser);
                return new AuthResponseModel<string> { Message = OperationResult.Success, Data = "Registration successful" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering new user {Email}", request.Email);
                return new AuthResponseModel<string> { Message = OperationResult.Failed, ErrorMessage = "Registration failed" };
            }
        }

        public async Task<AuthResponseModel<TokenDto>> Authenticate(LoginRequestModel request)
        {
            var user = await _repository.AuthenticateAsynce(request.Email);
            if (user == null)
                return new AuthResponseModel<TokenDto> { Message = OperationResult.NotFound, ErrorMessage = "Email not exists" };

            var verifyPassword = _passwordHasher.VerifyHashedPassword(user, user.PasswordHashing, request.Password);
            if (verifyPassword == PasswordVerificationResult.Failed)
                return new AuthResponseModel<TokenDto> { Message = OperationResult.Failed, ErrorMessage = "Invalid password" };

            var dto = new IdentityDto
            {
                Id = user.Id,
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role
            };

            return await _tokenManager.GenerateTokenAsync(dto);
        }

        public async Task<AuthResponseModel<string>> RegisterWithUserApiAsync(RegisterRequestModel request)
        {
            var userCheck = new CheckUserModel
            {
                Email = request.Email,
                Role = request.Role
            };

            // Gọi sang UserService (đã được map thành RegisterUserResponseModel trong UserApiService)
            var response = await _userApiService.RegisterUserAsync(userCheck);

            if (response == null)
            {
                return new AuthResponseModel<string>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "User service returned null or invalid response"
                };
            }

            if (response.StatusCode == 200)
            {
                var userId = response.UserId;

                return await RegisterAsync(request, userId);
            }

            if (response.StatusCode == 400)
                return new AuthResponseModel<string> { Message = OperationResult.Failed, ErrorMessage = "Email already exists" };

            if (response.StatusCode == 422) // ví dụ nếu bạn muốn handle invalid request
                return new AuthResponseModel<string> { Message = OperationResult.Failed, ErrorMessage = "Invalid request to user service" };

            if (response.StatusCode == 404)
                return new AuthResponseModel<string> { Message = OperationResult.Error, ErrorMessage = "User service unavailable" };

            return new AuthResponseModel<string> { Message = OperationResult.Error, ErrorMessage = "Unexpected error" };
        }

    }
}

using BLL.External;
using BLL.Services.Interfaces;
using DAL.Repository.Interfaces;
using Commons.Enums;
using DAL.Models.Entities;
using Commons.Models.Requests;
using Commons.Models.Responses;
using Microsoft.AspNetCore.Identity;
using System.Net;
using static Commons.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace BLL.Services
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

            var response = await _userApiService.RegisterUserAsync(userCheck);

            if (response.Message == OperationResult.Success)
            {
                var userId = response.Data;
                return await RegisterAsync(request, userId);
            }

            return new AuthResponseModel<string>
            {
                Message = response.Message,
                ErrorMessage = response.ErrorMessage
            };
        }

    }
}

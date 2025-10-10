using Auth.BLL.External;
using Auth.BLL.Services.Interfaces;
using Auth.DAL.Repository.Interfaces;
using Auth.Common.Enums;
using Auth.DAL.Models.Entities;
using Auth.Common.Models.Requests;
using Auth.Common.Models.Responses;
using Microsoft.AspNetCore.Identity;
using static Auth.Common.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace Auth.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repository;
        private readonly IPasswordHasher<IdentityModel> _passwordHasher;
        private readonly ITokenManager _tokenManager;
        private readonly UserApiService _userApiService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IAuthRepository repository,
                           IPasswordHasher<IdentityModel> passwordHasher,
                           ITokenManager tokenManager,
                           UserApiService userApiService,
                           IEmailService emailService,
                           ILogger<AuthService> logger)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _tokenManager = tokenManager;
            _userApiService = userApiService;
            _emailService = emailService;
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

            //if (!user.IsVerified)
            //    return new AuthResponseModel<TokenDto> 
            //    { 
            //        Message = OperationResult.Failed, 
            //        ErrorMessage = "Email not verified",
            //        Data = new TokenDto
            //        {
            //            VerifyEmail = false,
            //        }
            //    };

            var verifyPassword = _passwordHasher.VerifyHashedPassword(user, user.PasswordHashing, request.Password);
            if (verifyPassword == PasswordVerificationResult.Failed)
                return new AuthResponseModel<TokenDto> { Message = OperationResult.Failed, ErrorMessage = "Invalid password" };

            // Check role based on client type
            if (request.ClientType.Equals("admin", StringComparison.OrdinalIgnoreCase) && user.Role != "seller")
            {
                return new AuthResponseModel<TokenDto>
                {
                    Message = OperationResult.Forbidden,
                    ErrorMessage = "Only sellers can login from Admin site"
                };
            }

            if (request.ClientType.Equals("frontend", StringComparison.OrdinalIgnoreCase) && user.Role != "buyer")
            {
                return new AuthResponseModel<TokenDto>
                {
                    Message = OperationResult.Forbidden,
                    ErrorMessage = "Only buyers can login from Frontend site"
                };
            }

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

            if (response.Message != OperationResult.Success)
            {
                return new AuthResponseModel<string>
                {
                    Message = response.Message,
                    ErrorMessage = response.ErrorMessage
                };
            }

            var userId = response.Data;
            var passwordHash = _passwordHasher.HashPassword(null!, request.Password);

            // ✅ Tạo mã xác minh
            //var verifyCode = new Random().Next(100000, 999999).ToString();
            //var expiry = DateTime.UtcNow.AddMinutes(10);

            var newUser = new IdentityModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = request.Email,
                Role = request.Role,
                //PasswordHashing = passwordHash,
                //VerificationCode = verifyCode,
                //VerificationExpiry = expiry
            };

            try
            {
                await _repository.CreateIdentityAsync(newUser);

                // ✅ Gửi email xác minh
                //await _emailService.SendVerificationEmailAsync(request.Email, verifyCode);

                return new AuthResponseModel<string>
                {
                    Message = OperationResult.Success,
                    Data = "Registration successful. Please check your email to verify your account."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user {Email}", request.Email);
                return new AuthResponseModel<string>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Registration failed"
                };
            }
        }

        //public async Task<AuthResponseModel<string>> VerifyEmailAsync(string email, string code)
        //{
        //    var user = await _repository.GetByEmailAsync(email);
        //    if (user == null)
        //        return new AuthResponseModel<string> { Message = OperationResult.NotFound, ErrorMessage = "User not found" };

        //    if (user.IsVerified)
        //        return new AuthResponseModel<string> { Message = OperationResult.Conflict, ErrorMessage = "Email already verified" };

        //    //if (user.VerificationCode != code)
        //    //    return new AuthResponseModel<string> { Message = OperationResult.Failed, ErrorMessage = "Invalid verification code" };

        //    if (user.VerificationExpiry < DateTime.UtcNow)
        //        return new AuthResponseModel<string> { Message = OperationResult.Failed, ErrorMessage = "Verification code expired" };

        //    user.IsVerified = true;
        //    user.VerifiedAt = DateTime.UtcNow;
        //    user.VerificationCode = null;
        //    user.VerificationExpiry = null;

        //    await _repository.UpdateVerificationAsync(user);

        //    return new AuthResponseModel<string>
        //    {
        //        Message = OperationResult.Success,
        //        Data = "Email verified successfully"
        //    };
        //}

        //public async Task<AuthResponseModel<string>> ResendVerificationCodeAsync(string email)
        //{
        //    var user = await _repository.GetByEmailAsync(email);

        //    if (user == null)
        //        return new AuthResponseModel<string>
        //        {
        //            Message = OperationResult.NotFound,
        //            ErrorMessage = "User not found"
        //        };

        //    if (user.IsVerified)
        //        return new AuthResponseModel<string>
        //        {
        //            Message = OperationResult.Conflict,
        //            ErrorMessage = "Email already verified"
        //        };


        //    if (user.VerificationExpiry != null && user.VerificationExpiry > DateTime.UtcNow)
        //    {
        //        var remaining = user.VerificationExpiry.Value.Subtract(DateTime.UtcNow).TotalMinutes;
        //        return new AuthResponseModel<string>
        //        {
        //            Message = OperationResult.Failed,
        //            ErrorMessage = $"Verification code still valid. Please wait {remaining:F0} minutes or check your email."
        //        };
        //    }

        //    // ✅ Tạo mã mới
        //    var newCode = new Random().Next(100000, 999999).ToString();
        //    user.VerificationCode = newCode;
        //    user.VerificationExpiry = DateTime.UtcNow.AddMinutes(10);

        //    await _repository.UpdateVerificationAsync(user);

        //    // ✅ Gửi email xác minh mới
        //    await _emailService.SendVerificationEmailAsync(user.Email, newCode);

        //    return new AuthResponseModel<string>
        //    {
        //        Message = OperationResult.Success,
        //        Data = "A new verification code has been sent to your email."
        //    };
        //}



        public AuthResponseModel<string> GetTokenSystemAsync()
        {
            var sysToken = _tokenManager.GenerateInternalServiceToken();

            return new AuthResponseModel<string>
            {
                Message = OperationResult.Success,
                Data = sysToken
            };
        }
    }
}

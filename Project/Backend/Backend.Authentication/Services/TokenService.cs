using Backend.Authentication.Enums;
using Backend.Authentication.Models;
using Backend.Authentication.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Backend.Authentication.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IAuthRepository _authRepository;
        private readonly IPasswordHasher<IdentityModel> _passwordHasher = new PasswordHasher<IdentityModel>();
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration config, IAuthRepository authRepository,
                            IPasswordHasher<IdentityModel> pwdHaser, ILogger<TokenService> logger)
        {
            _config = config;
            _authRepository = authRepository;
            _passwordHasher = pwdHaser;
            _logger = logger;
        }

        public async Task<LoginResponseModel> RegisterAsync(RegisterRequestModel request, Guid userId)
        {
            var newUser = new IdentityModel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = request.Email,
                Role = request.Role,
            };

            newUser.PasswordHashing = _passwordHasher.HashPassword(newUser, request.Password);

            try
            {
                await _authRepository.CreateIdentityAsync(newUser);
                return new LoginResponseModel
                {
                    ErrorMessage = "Registration Successfully!",
                    Message = OperationResult.Success
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering new user {Email}", request.Email);
                return new LoginResponseModel
                {
                    ErrorMessage = "Registration failed",
                    Message = OperationResult.Failed
                };
            }
        }



        public async Task<LoginResponseModel> Authenticate(LoginRequestModel request)
        {

            var result = await _authRepository.AuthenticateAsynce(request.Email);

            if (result == null)
            {
                return new LoginResponseModel
                {
                    ErrorMessage =  "Email not exists!",
                    Message = OperationResult.NotFound,
                };
            }

            var verifyPassword = _passwordHasher.VerifyHashedPassword(result, result.PasswordHashing, request.Password);

            if (verifyPassword == PasswordVerificationResult.Failed)
            {
                return new LoginResponseModel
                {
                    ErrorMessage = "Invalid Password!",
                    Message = OperationResult.Failed,
                };
            }

            var access_token = await GenerateTokenAsync(result);


            try
            {
                await _authRepository.UpdateIdentityAsync(result); // bạn cần implement hàm UpdateIdentityAsync trong repository
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating RefreshTokenId for {Email}", request.Email);
            }

            return access_token;


        }


        public async Task<LoginResponseModel> GenerateTokenAsync(IdentityModel identity)
        {
            try
            {
                var jwt = _config.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var tokenExpiry = DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenExpiresMinutes"]!));

                var ResfreshTokenDays = int.Parse(jwt["RefreshTokenExpiresDays"]!);

                var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, identity.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, identity.Role)
            };

                var token = new JwtSecurityToken(
                    issuer: jwt["Issuer"],
                    audience: jwt["Audience"],
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: tokenExpiry,
                    signingCredentials: creds
                );

                var access_token = new JwtSecurityTokenHandler().WriteToken(token);

                return new LoginResponseModel
                {
                    AccessToken = access_token,
                    ExpiresIn = (int)tokenExpiry.Subtract(DateTime.UtcNow).TotalSeconds,
                    TokenType = "Bearer",
                    Roles = identity.Role,
                    RefreshToken = await RefreshTokenAsync(identity, ResfreshTokenDays),
                    Message = OperationResult.Success
                };
            }

            catch (Exception ex)
            {
                return new LoginResponseModel
                {
                    ErrorMessage = ex.Message,
                    Message = OperationResult.Error,
                };
            }
        }


        public async Task<RefreshTokenResponseModel> RefreshTokenAsync(IdentityModel identity, int refreshTokenDays)
        {
            try
            {
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenDays);
                var sessionExpiry = DateTime.UtcNow.AddHours(8); // session 8 hour
                var rawToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
                var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));

                var refreshToken = new RefreshTokenModel
                {
                    RefreshTokenId = Guid.NewGuid(),
                    IdentityId = identity.Id,
                    TokenHash = hash,
                    ExpiryDate = refreshTokenExpiry,
                    SessionExpiry = sessionExpiry,
                    LastActivity = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                await _authRepository.CreateRefreshTokenAsync(refreshToken);

                return new RefreshTokenResponseModel
                {
                    RawToken = rawToken,
                    ExpiresIn = (int)refreshTokenExpiry.Subtract(DateTime.UtcNow).TotalSeconds,
                    Message = OperationResult.Success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refresh token for {UserId}", identity.UserId);
                return new RefreshTokenResponseModel
                {
                    ErrorMessage = "Internal server error",
                    Message = OperationResult.Error
                };
            }
        }

        public string GenerateInternalServiceToken(string serviceName = "InternalService")
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenExpiry = DateTime.UtcNow.AddMinutes(
                int.Parse(jwt["InternalTokenExpiresMinutes"] ?? "5")
            );

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, serviceName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "system")  
        };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: tokenExpiry,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Generated internal token for {Service} with role=system", serviceName);

            return tokenString;
        }


        public async Task<LoginResponseModel> ValidateRefreshTokenAsync(string rawToken)
        {
            try
            {
                var hashingToken = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
                var refreshToken = await _authRepository.GetRefreshTokenAsync(hashingToken);

                if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new LoginResponseModel
                    {
                        ErrorMessage = "Invalid or expired refresh token",
                        Message = OperationResult.Failed
                    };
                }

                // check session absolute/sliding
                if (refreshToken.SessionExpiry < DateTime.UtcNow)
                {
                    return new LoginResponseModel
                    {
                        ErrorMessage = "Session expired",
                        Message = OperationResult.Failed
                    };
                }

                var user = await _authRepository.FindUserByTokenIdAsync(refreshToken.IdentityId);
                if (user == null)
                {
                    return new LoginResponseModel
                    {
                        ErrorMessage = "User not found",
                        Message = OperationResult.NotFound
                    };
                }

                // revoke old token
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _authRepository.UpdateRefreshTokenAsync(refreshToken);

                // cấp mới và extend session
                var newAccessToken = await GenerateTokenAsync(user);

                return newAccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token");
                return new LoginResponseModel
                {
                    ErrorMessage = "Internal server error",
                    Message = OperationResult.Error
                };
            }
        }


    }
}

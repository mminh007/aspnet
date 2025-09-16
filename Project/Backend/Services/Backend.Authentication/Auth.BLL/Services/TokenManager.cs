using Auth.BLL.Services.Interfaces;
using Auth.Common.Enums;
using Auth.Common.Models.Responses;
using Auth.DAL.Models.Entities;
using Auth.DAL.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using static Auth.Common.Models.DTOs;

namespace Auth.Services
{
    public class TokenManager: ITokenManager
    {
        private readonly IConfiguration _config;
        private readonly IAuthRepository _repository;
        private readonly ILogger<TokenManager> _logger;

        public TokenManager(IConfiguration config, IAuthRepository repository, ILogger<TokenManager> logger)
        {
            _config = config;
            _repository = repository;
            _logger = logger;
        }

        public async Task<AuthResponseModel<TokenDto>> GenerateTokenAsync(IdentityDto identity)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenExpiry = DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenExpiresMinutes"]!));
            var refreshTokenDays = int.Parse(jwt["RefreshTokenExpiresDays"]!);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, identity.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, identity.Role),
                //new Claim("role", identity.Role),                   // Ocelot standard  
                //new Claim("Role", identity.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: tokenExpiry,
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Tạo refresh token
            var rawToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));

            var refreshToken = new RefreshTokenModel
            {
                RefreshTokenId = Guid.NewGuid(),
                IdentityId = identity.Id,
                TokenHash = hash,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenDays),
                SessionExpiry = DateTime.UtcNow.AddHours(8),
                LastActivity = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _repository.CreateRefreshTokenAsync(refreshToken);

            return new AuthResponseModel<TokenDto>
            {
                Message = OperationResult.Success,
                Data = new TokenDto
                {
                    AccessToken = accessToken,
                    RefreshToken = rawToken,
                    ExpiresIn = (int)tokenExpiry.Subtract(DateTime.UtcNow).TotalSeconds,
                    Roles = identity.Role
                }
            };
        }

        public async Task<AuthResponseModel<TokenDto>> ValidateRefreshTokenAsync(string rawToken)
        {
            var hashingToken = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            var refreshToken = await _repository.GetRefreshTokenAsync(hashingToken);

            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiryDate < DateTime.UtcNow)
                return new AuthResponseModel<TokenDto> { Message = OperationResult.Failed, ErrorMessage = "Invalid or expired refresh token" };

            if (refreshToken.SessionExpiry < DateTime.UtcNow)
                return new AuthResponseModel<TokenDto> { Message = OperationResult.Failed, ErrorMessage = "Session expired" };

            var user = await _repository.FindUserByTokenIdAsync(refreshToken.IdentityId);
            if (user == null)
                return new AuthResponseModel<TokenDto> { Message = OperationResult.NotFound, ErrorMessage = "User not found" };

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _repository.UpdateRefreshTokenAsync(refreshToken);

            var dto = new IdentityDto
            {
                Id = user.Id,
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role
            };

            return await GenerateTokenAsync(dto);
        }

        public string GenerateInternalServiceToken(string serviceName = "InternalService")
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenExpiry = DateTime.UtcNow.AddMinutes(int.Parse(jwt["InternalTokenExpiresMinutes"] ?? "5"));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, serviceName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "system"),
                //new Claim("role", "system"),                   // Ocelot standard  
                //new Claim("Role", "system")                     // Ocelot standard 
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: tokenExpiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}


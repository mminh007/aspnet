using Backend.Authentication.Enums;

namespace Backend.Authentication.Models
{
    public class LoginResponseModel
    {
        public Guid UserId { get; set; }

        public string? AccessToken { get; set; }

        public int? ExpiresIn { get; set; }

        public string? TokenType { get; set; }

        public string Roles { get; set; }
        public RefreshTokenResponseModel? RefreshToken { get; set; }

        public string? ErrorMessage { get; set; }

        public OperationResult Message { get; set; }
    }
}

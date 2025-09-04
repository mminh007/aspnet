using Backend.Authentication.Enums;

namespace Backend.Authentication.Models
{
    public class RefreshTokenResponseModel
    {
        public Guid TokenId { get; set; }

        public int ExpiresIn { get; set; }

        public string RawToken { get; set; }

        public string? ErrorMessage { get; set; }   

        public OperationResult Message { get; set; }
    }
}

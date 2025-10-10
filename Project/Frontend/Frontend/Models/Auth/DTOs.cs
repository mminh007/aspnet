using System.Text.Json.Serialization;

namespace Frontend.Models.Auth
{
    public class DTOs
    {
        public class TokenData
        {
            [JsonPropertyName("accessToken")] public string AccessToken { get; set; }
            [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; }
            [JsonPropertyName("expiresIn")] public int ExpiresIn { get; set; }
            [JsonPropertyName("roles")] public string Roles { get; set; }

            [JsonPropertyName("verifyEmail")] public bool? VerifyEmail { get; set; }
        }
    }
}

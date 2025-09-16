using System.Text.Json.Serialization;

namespace Auth.Common.Models.Responses
{
    public class RegisterUserResponseModel
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("data")]
        public Guid UserId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}

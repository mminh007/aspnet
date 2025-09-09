using System.Text.Json.Serialization;

namespace Commons.Models.Responses
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

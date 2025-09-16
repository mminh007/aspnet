using Auth.Common.Enums;
using System.Text.Json.Serialization;

namespace Auth.Common.Models.Responses
{
    public class AuthResponseModel<T>
    {
        public OperationResult Message { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}

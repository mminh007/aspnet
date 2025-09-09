using Commons.Enums;
using System.Text.Json.Serialization;

namespace Commons.Models.Responses
{
    public class AuthResponseModel<T>
    {
        public OperationResult Message { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}

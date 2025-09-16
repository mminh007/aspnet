using System.Text.Json.Serialization;
using User.Common.Enums;

namespace User.Common.Models.Responses
{
    public class UserApiResponse<T>
    {
        public OperationResult Message { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}

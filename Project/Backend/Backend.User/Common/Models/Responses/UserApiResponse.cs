using Commons.Enums;

namespace Commons.Models.Responses
{
    public class UserApiResponse<T>
    {
        public OperationResult Message { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}

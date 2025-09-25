using Backend.Authentication.Enums;

namespace Backend.Authentication.Models.Responses
{
    public class AuthResponseModel<T>
    {
        public OperationResult Message { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}

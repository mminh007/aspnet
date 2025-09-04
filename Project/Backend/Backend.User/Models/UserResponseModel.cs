using Backend.User.Enums;

namespace Backend.User.Models
{
    public class UserResponseModel
    {
        public Guid UserId { get; set; }

        public bool Success { get; set; }

        public OperationResult Message { get; set; }

        public string? ErrorMessage { get; set; }
    }
}

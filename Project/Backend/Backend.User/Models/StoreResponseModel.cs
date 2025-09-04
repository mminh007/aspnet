using Backend.User.Enums;

namespace Backend.User.Models
{
    public class StoreResponseModel
    {
        public bool Success { get; set; }

        public OperationResult Message { get; set; }

        public string? ErrorMessage { get; set; }

    }
}

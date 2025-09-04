using Backend.Store.Enums;

namespace Backend.Store.Models
{
    public class StoreResponseModel
    {
        public bool Success { get; set; }

        public OperationResult Message { get; set; }

        public string? ErrorMessage { get; set; }

        public IEnumerable<StoreDTO>? StoreList { get; set; }

        public StoreDTO? StoreInfo { get; set; }
    }
}

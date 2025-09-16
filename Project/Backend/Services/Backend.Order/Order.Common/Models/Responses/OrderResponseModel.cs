using Order.Common.Enums;


namespace Order.Common.Models.Responses
{
    public class OrderResponseModel<T>
    {
        public bool Success { get; set; }
        public OperationResult Message { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}

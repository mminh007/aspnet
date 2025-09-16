using Store.Common.Enums;

namespace Store.Common.Models.Responses
{
    public class StoreResponseModel<T>
    {

        public OperationResult Message { get; set; }

        public string? ErrorMessage { get; set; }


        public T? Data { get; set; }
    }
}

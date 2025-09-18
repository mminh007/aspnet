namespace Frontend.Models.Orders.Requests
{
    public class RequestItemsToCartModel
    {
        public Guid StoreId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

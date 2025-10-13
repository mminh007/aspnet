namespace Frontend.Models.Orders.Requests
{
    public class RequestUpdate
    {
        public Guid Productid { get; set; }
        public Guid CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}

namespace Frontend.Models.Orders.Requests
{
    public class UpdateQuantityModel
    {
        public Guid Productid { get; set; }
        public int Quantity { get; set; }
    }
}

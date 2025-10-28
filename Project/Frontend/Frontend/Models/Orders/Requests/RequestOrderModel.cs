namespace Frontend.Models.Orders.Requests
{
    public class RequestOrderModel
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;

        public IEnumerable<Guid> ProductIds { get; set; } = new List<Guid>();
    }
}

namespace Frontend.Configs.Order
{
    public class OrderEndpoints
    {
        public string GetCart { get; set; } = string.Empty;
        public string GetCountingItems { get; set; } = string.Empty;
        public string AddItemsToCart { get; set; } = string.Empty;
        public string UpdateItemQuantity { get; set; } = string.Empty;
        public string RemoveItem { get; set; } = string.Empty;
        public string ClearCart { get; set; } = string.Empty;

        public string GetOrderById { get; set; } = string.Empty;
        public string GetOrdersByUser { get; set; } = string.Empty;
        public string DeleteOrder { get; set; } = string.Empty;
        public string Checkout { get; set; } = string.Empty;
    }
}

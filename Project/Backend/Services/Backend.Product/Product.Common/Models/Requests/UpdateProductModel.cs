

namespace Common.Models.Requests
{
    public class UpdateProductModel
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string ProductImage { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? ImportPrice { get; set; }
        public int? Quantity { get; set; }
        public string? Supplier { get; set; }
        public bool? IsActive { get; set; }

        public Guid? CategoryId { get; set; }
    }
}

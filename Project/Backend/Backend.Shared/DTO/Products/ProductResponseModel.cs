using Backend.Shared.Enums;

namespace Backend.Shared.DTO.Products
{
    public class ProductResponseModel
    {
        public bool Success { get; set; }
        public OperationResult Message { get; set; }
        public string? ErrorMessage { get; set; }

        // ---------------------------
        // Output for Buyer
        // ---------------------------
        public ProductBuyerDTO? BuyerProduct { get; set; }
        public IEnumerable<ProductBuyerDTO>? BuyerProductList { get; set; }

        // ---------------------------
        // Output for Seller
        // ---------------------------
        public ProductSellerDTO? SellerProduct { get; set; }
        public IEnumerable<ProductSellerDTO>? SellerProductList { get; set; }

        // ---------------------------
        // Category for Seller
        // ---------------------------
        public CategoryDTO? CategoryInfo { get; set; }
        public IEnumerable<CategoryDTO>? CategoryList { get; set; }

        public List<Guid>? NotFoundProductIds { get; set; }
    }
}

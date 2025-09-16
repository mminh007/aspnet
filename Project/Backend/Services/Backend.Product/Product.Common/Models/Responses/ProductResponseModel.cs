
using Common.Enums;

namespace Common.Models.Responses
{
    public class ProductResponseModel<T>
    {
        public bool Success { get; set; }
        public OperationResult Message { get; set; }
        public string? ErrorMessage { get; set; }

        // ---------------------------
        // Output for Buyer
        // ---------------------------
        // public DTOs.ProductBuyerDTO? BuyerProduct { get; set; }
        // public IEnumerable<DTOs.ProductBuyerDTO>? BuyerProductList { get; set; }

        // ---------------------------
        // Output for Seller
        // ---------------------------
        // public DTOs.ProductSellerDTO? SellerProduct { get; set; }
        // public IEnumerable<DTOs.ProductSellerDTO>? SellerProductList { get; set; }

        // ---------------------------
        // Category for Seller
        // ---------------------------
        // public DTOs.CategoryDTO? CategoryInfo { get; set; }
        // public IEnumerable<DTOs.CategoryDTO>? CategoryList { get; set; }

        public List<Guid>? NotFoundProductIds { get; set; }

        public T? Data { get; set; }
    }
}

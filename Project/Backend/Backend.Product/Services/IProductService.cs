using Backend.Product.Enums;
using Backend.Product.Models;
using System.Security.Claims;
using Backend.Shared.DTO.Products;


namespace Backend.Product.Services
{
    public interface IProductService
    {
        // Buyer
        // ---------------------------
        // Task<ProductResponseModel> GetProductsByCategoryForBuyerAsync(Guid categoryId);
        // Task<ProductResponseModel> SearchProductsForBuyerAsync(string keyword);

        // ---------------------------
        // Seller
        // ---------------------------
        Task<ProductResponseModel> GetProductByIdAsync(Guid productId);
        Task<ProductResponseModel> GetProductsByStoreAsync(Guid storeId, ClaimsPrincipal user);
        Task<ProductResponseModel> GetProductsByStoreAndCategoryAsync(Guid storeId, Guid categoryId);
        Task<ProductResponseModel> SearchProductsByStoreAsync(Guid storeId, string keyword);

        Task<ProductResponseModel> CreateProductAsync(ProductDTOModel product);
        Task<ProductResponseModel> UpdateProductAsync(IEnumerable<UpdateProductModel> dto);
        Task<ProductResponseModel> DeleteProductAsync(Guid productId);

        // ---------------------------
        // Category 
        // ---------------------------
        Task<ProductResponseModel> CreateCategoryAsync(CategoryDTO category);
        Task<ProductResponseModel> SearchCategoriesAsync(Guid storeId);
    }
}

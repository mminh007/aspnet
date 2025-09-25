using Backend.Product.Enums;
using Backend.Product.Models.Requests;
using System.Security.Claims;
using Backend.Product.Models.Responses;
using Backend.Product.Models;


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

        Task<ProductResponseModel> CreateProductAsync(DTOs.ProductDTO product);
        Task<ProductResponseModel> UpdateProductAsync(IEnumerable<UpdateProductModel> dto);
        Task<ProductResponseModel> DeleteProductAsync(Guid productId);

        // ---------------------------
        // Category 
        // ---------------------------
        Task<ProductResponseModel> CreateCategoryAsync(DTOs.CategoryDTO category);
        Task<ProductResponseModel> SearchCategoriesAsync(Guid storeId);
    }
}

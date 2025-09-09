using Common.Models.Requests;
using System.Security.Claims;
using Common.Models.Responses;
using Common.Models;


namespace BLL.Services.Interfaces
{
    public interface IProductService
    {
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

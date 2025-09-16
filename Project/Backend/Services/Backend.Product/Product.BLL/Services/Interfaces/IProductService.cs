using Common.Models.Requests;
using System.Security.Claims;
using Common.Models.Responses;
using Common.Models;


namespace BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseModel<DTOs.ProductSellerDTO>> GetProductByIdAsync(Guid productId);
        Task<ProductResponseModel<IEnumerable<object>>> GetProductsByStoreAsync(Guid storeId, string userRole);
        Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> GetProductsByStoreAndCategoryAsync(Guid storeId, Guid categoryId);
        Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> SearchProductsByStoreAsync(Guid storeId, string keyword);

        Task<ProductResponseModel<DTOs.ProductSellerDTO>> CreateProductAsync(DTOs.ProductDTO product);
        Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> UpdateProductAsync(IEnumerable<UpdateProductModel> dto);
        Task<ProductResponseModel<string>> DeleteProductAsync(Guid productId);

        Task<ProductResponseModel<DTOs.OrderProductDTO>> OrderGetProductInfo(Guid productId);

        // ---------------------------
        // Category 
        // ---------------------------
        Task<ProductResponseModel<DTOs.CategoryDTO>> CreateCategoryAsync(DTOs.CategoryDTO category);
        Task<ProductResponseModel<IEnumerable<DTOs.CategoryDTO>>> SearchCategoriesAsync(Guid storeId);
    }
}

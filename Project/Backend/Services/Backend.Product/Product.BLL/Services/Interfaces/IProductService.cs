using Common.Models.Requests;
using System.Security.Claims;
using Common.Models.Responses;
using Common.Models;
using Product.Common.Models.Requests;


namespace BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseModel<DTOs.ProductSellerDTO>> GetProductByIdAsync(Guid productId);
        Task<ProductResponseModel<IEnumerable<object>>> GetProductsByStoreAsync(Guid storeId, string userRole);
        Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> GetProductsByStoreAndCategoryAsync(Guid storeId, Guid categoryId);
        Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> SearchProductsByStoreAsync(Guid storeId, string keyword);

        Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> CreateProductAsync(DTOs.ProductDTO product);
        Task<ProductResponseModel<IEnumerable<DTOs.ProductSellerDTO>>> UpdateProductAsync(UpdateProductModel dto);
        Task<ProductResponseModel<string>> DeleteProductAsync(Guid productId);

        Task<ProductResponseModel<DTOs.OrderProductDTO>> OrderGetProductInfo(Guid productId);
        Task<ProductResponseModel<IEnumerable<DTOs.OrderProductDTO>>> OrderGetProductInfo2(List<Guid> productId);

        Task<ProductResponseModel<object>> ChangeActiveProductAsync(ChangeActiveProduct request);

        // ---------------------------
        // Category 
        // ---------------------------
        Task<ProductResponseModel<DTOs.CategoryDTO>> CreateCategoryAsync(DTOs.CategoryDTO category);
        Task<ProductResponseModel<IEnumerable<DTOs.CategoryDTO>>> SearchCategoriesAsync(Guid storeId);

        Task<ProductResponseModel<int>> DeleteCategoryAsync(Guid categoryId);
    }
}

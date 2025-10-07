using Adminstrator.Models.Products;
using Adminstrator.Models.Products.Requests;

namespace Adminstrator.HttpsClients.Interfaces
{
    public interface IProductApiClient
    {
        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductSellerDTO>? Data)> GetByStoreAsync(Guid storeId);

        Task<(bool Success, string? Message, int statusCode, DTOs.ProductSellerDTO? Data)> GetByIdAsync(Guid productId);

        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.CategoryDTO>? Data)> SearchCategoriesAsync(Guid storeId);

        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductSellerDTO>? Data)> UpdateProductAsync(Guid productId, UpdateProductModel model);

        Task<(bool Success, string? Message, int statusCode)> CreateProductAsync(DTOs.ProductDTO model);

        Task<(bool Success, string? Message, int statusCode, string data)> DeleteProductAsync(Guid productId);

        Task<(bool Success, string? Message, int statusCode)> UpdateActiveAsync(ChangeActiveProduct model);

        Task<(bool Success, string? Message, int statusCode)> CreateCategoryAsync(DTOs.CategoryDTO category);

        Task<(bool Success, string? Message, int statusCode)> DeleteCategoryAsync(Guid categoryId);
    }
}

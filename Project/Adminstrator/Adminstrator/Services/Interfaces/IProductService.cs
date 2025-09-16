using Adminstrator.Models.Products;

namespace Adminstrator.Services.Interfaces
{
    public interface IProductService
    {
        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductSellerDTO>? Data)> GetByStoreAsync(Guid storeId);

        Task<(bool Success, string? Message, int statusCode, DTOs.ProductSellerDTO? Data)> GetByIdAsync(Guid productId);

        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.CategoryDTO>? Data)> SearchCategoriesAsync(Guid storeId);
    }
}

using Frontend.Models.Products;

namespace Frontend.HttpsClients.Products
{
    public interface IProductApiClient
    {
        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductBuyerDTO>? Data)> GetByStoreAsync(Guid storeId);

        Task<(bool Success, string? Message, int statusCode, DTOs.ProductBuyerDTO? Data)> GetByIdAsync(Guid productId);

        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.CategoryDTO>? Data)> SearchCategoriesAsync(Guid storeId);
    }
}

using Frontend.Models.Products;

namespace Frontend.Services.Interfaces
{
    public interface IProductService
    {
        Task<(string Message, int StatusCode, IEnumerable<DTOs.ProductBuyerDTO?>)> GetProductByStoreIdAsync(Guid storeId);

        //Task<(string Message, int StatusCode, DTOs.ProductBuyerDTO?)> GetProductByIdAsync(Guid productId);
        Task<(string Message, int StatusCode, IEnumerable<DTOs.CategoryDTO?>)> SearchCategoriesByStoreIdAsync(Guid storeId);
        Task ClearProductsCache(Guid storeId);
    }
}

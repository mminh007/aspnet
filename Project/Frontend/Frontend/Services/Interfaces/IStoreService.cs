
using Frontend.Models.Stores;

namespace Frontend.Services.Interfaces
{
    public interface IStoreService
    {
        Task<(string message, int statusCode, IEnumerable<StoreDto?>)> GetStoresPagedAsync(int page, int pageSize);

        Task<(string message, int statusCode, StoreDto?)> GetStoresDetailAsync(Guid storeId);

        Task<(string message, int statusCode, IEnumerable<StoreDto?>)> GetStoreByKeywordAsync(string keyword);

        Task<(string message, int statusCode, IEnumerable<StoreDto?>)> GetStoreByTagAsync(string tag);
    }
}

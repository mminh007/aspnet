
using Frontend.Models.Stores;

namespace Frontend.Services.Interfaces
{
    public interface IStoreService
    {
        Task<(string message, int statusCode, IEnumerable<StoreDto?> data)> GetStoresPagedAsync(int page, int pageSize);

        Task<(string message, int statusCode, StoreDto? data)> GetStoresDetailAsync(Guid storeId);

        Task<(string message, int statusCode, IEnumerable<StoreDto?> data)> GetStoreByKeywordAsync(string keyword);

        Task<(string message, int statusCode, PaginatedStoreResponse data)> GetStoresByTagPagedAsync(string tag, int page = 1, int pageSize = 9);
    }
}

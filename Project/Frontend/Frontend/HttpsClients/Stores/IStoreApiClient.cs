using Frontend.Models.Stores;

namespace Frontend.HttpsClients.Stores
{
    public interface IStoreApiClient
    {
        Task<(bool Success, string? Message, int statusCode, IEnumerable<StoreDto?> Data)> GetStoresPagedAsync(int page, int pageSize);
        Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetStoreByIdAsync(Guid storeId);

        Task<(bool Success, string? Message, int statusCode, PaginatedStoreResponse data)> SearchStoreByKeywordAsync(string keyword, int page, int pageSize);

        Task<(bool Success, string? Message, int statusCode, PaginatedStoreResponse data)> SearchStoreByTag(string tag, int page, int pageSize);
    }
}

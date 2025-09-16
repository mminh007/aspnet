using Frontend.Models.Stores;

namespace Frontend.HttpsClients.Stores
{
    public interface IStoreApiClient
    {
        Task<(bool Success, string? Message, int statusCode, IEnumerable<StoreDto?> Data)> GetAllStoresActiveAsync();

    }
}

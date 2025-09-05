using Frontend.Models.Stores;

namespace Frontend.HttpsClients.Stores
{
    public interface IStoreApiClient
    {
        Task<List<StoreResponseModel>> GetStoresAsync();
    }
}


using Frontend.Models.Stores;

namespace Frontend.Services.Interfaces
{
    public interface IStoreService
    {
        Task<(string message, int statusCode, IEnumerable<StoreDto?>)> GetAllStoresActiveAsync();

        Task<(string message, int statusCode, StoreDto?)> GetStoresDetailAsync(Guid storeId);
    }
}

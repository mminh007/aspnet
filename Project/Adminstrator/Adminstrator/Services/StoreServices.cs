using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Stores;
using Adminstrator.Services.Interfaces;

namespace Adminstrator.Services
{
    public class StoreServices: IStoreServices
    {
        private readonly IStoreApiClient _storeApi;

        public StoreServices(IStoreApiClient storeApi)
        {
            _storeApi = storeApi;
        }

        public async Task<(string message, int statusCode, StoreDto?)> GetStoreByIdAsync(Guid storeId)
        {
            var (success, message, statusCode, data) = await _storeApi.GetByIdAsync(storeId);
            return (message, statusCode, data);
        }

        public async Task<(string message, int statusCode, StoreDto?)> GetStoreByUserIdAsync(Guid userId)
        {
            var (success, message, statusCode, data) = await _storeApi.GetByUserIdAsync(userId);        

            return (message, statusCode, data);
        }
    }
}

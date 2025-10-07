using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Stores;
using Adminstrator.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Adminstrator.Services
{
    public class StoreServices: IStoreService
    {
        private readonly IStoreApiClient _storeApi;

        public StoreServices(IStoreApiClient storeApi)
        {
            _storeApi = storeApi;
        }

        public async Task<(string message, int statusCode)> ChangeActiveStoreAsync(ChangeActiveRequest request)
        {
            var (success, message, statusCode) = await _storeApi.ChangeActiveStore(request);
            return (message, statusCode);

        }

        public async Task<(string message, int statusCode, StoreDto? data)> GetStoreByIdAsync(Guid storeId)
        {
            var (success, message, statusCode, data) = await _storeApi.GetByIdAsync(storeId);
            return (message, statusCode, data);
        }

        public async Task<(string message, int statusCode, StoreDto? data)> GetStoreByUserIdAsync()
        {
            var (success, message, statusCode, data) = await _storeApi.GetByUserIdAsync();        

            return (message, statusCode, data);
        }

        public async Task<(string message, int statusCode, StoreDto data)> UpdateStoreAsync(UpdateStoreModel model)
        {
            var (success, message, statusCode, data) = await _storeApi.UpdateInfomationStore(model);

            return (message, statusCode, data);
        }
    }
}

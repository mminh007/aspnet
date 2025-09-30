using Adminstrator.Models.Stores;

namespace Adminstrator.HttpsClients.Interfaces
{
    public interface IStoreApiClient
    {
        Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByUserIdAsync();
        Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByIdAsync(Guid storeId);

        Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> UpdateInfomationStore(UpdateStoreModel model);

        Task<(bool Success, string? Message, int statusCode)> ChangeActiveStore(ChangeActiveRequest request);


    }
}

using Adminstrator.Models.Stores;

namespace Adminstrator.HttpsClients.Interfaces
{
    public interface IStoreApiClient
    {
        Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByUserIdAsync(Guid userId);
        Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByIdAsync(Guid storeId);
    }
}

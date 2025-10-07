using Adminstrator.Models.Stores;
using Microsoft.AspNetCore.Http;

namespace Adminstrator.Services.Interfaces
{
    public interface IStoreService
    {
        Task<(string message, int statusCode, StoreDto? data)> GetStoreByUserIdAsync();

        Task<(string message, int statusCode, StoreDto? data)> GetStoreByIdAsync(Guid storeId);

        Task<(string message, int statusCode, StoreDto data)> UpdateStoreAsync(UpdateStoreModel model);

        Task<(string message, int statusCode)> ChangeActiveStoreAsync(ChangeActiveRequest request);
    }
}

using Adminstrator.Models.Stores;
using Microsoft.AspNetCore.Http;

namespace Adminstrator.Services.Interfaces
{
    public interface IStoreServices
    {
        Task<(string message, int statusCode, StoreDto?)> GetStoreByUserIdAsync(Guid userId);

        Task<(string message, int statusCode, StoreDto?)> GetStoreByIdAsync(Guid storeId);
    }
}

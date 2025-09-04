using Backend.Store.Models;

namespace Backend.Store.Services
{
    public interface IStoreService
    {
        Task<StoreResponseModel> CreateStoreAsync(RegisterStoreModel model);

        Task<StoreResponseModel> UpdateStoreAsync(UpdateStoreModel model);

        Task<StoreResponseModel> DeleteStoreAsync(Guid UserId);
        Task<StoreResponseModel> GetStoreByIdAsync(Guid UserId);

        Task<StoreResponseModel> ChangeActive(StoreActiveModel model);

        Task<StoreResponseModel> GetAllActiveStoresAsync();

        Task<StoreResponseModel> GetStoreDetailByIdAsync(Guid storeId);
    }
}

using Backend.Store.Models;

namespace Backend.Store.Services
{
    public interface IStoreService
    {
        Task<StoreResponseModel> CreateStoreAsync(RegisterStoreModel model);

        Task<StoreResponseModel> UpdateStoreAsync(UpdateStoreModel model);

        Task<StoreResponseModel> DeleteStoreAsync(Guid storeId);
        Task<StoreResponseModel> GetStoreByIdAsync(Guid storeId);

        Task<StoreResponseModel> ChangeActive(StoreActiveModel model);

        Task<StoreResponseModel> GetAllActiveStoresAsync();

        Task<StoreResponseModel> GetStoreDetailByIdAsync(Guid storeId);
    }
}

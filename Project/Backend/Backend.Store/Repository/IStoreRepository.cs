using Backend.Store.Models;

namespace Backend.Store.Repository
{
    public interface IStoreRepository
    {
        Task<int> UpdateStoreAsync(UpdateStoreModel model);

        Task<int> DeleteStoreAsync(Guid UserId);

        Task<Guid> CreateStoreAsync(RegisterStoreModel user);

        Task<StoreDTO> GetStoreInfo(Guid userId); // StoreDTO contain info Store

        Task<StoreDTO> GetStoreByKeyword(string keyword);

        Task<IEnumerable<StoreDTO>> GetAllActiveStoresAsync();

        Task<StoreDTO?> GetStoreDetailById(Guid storeId);


        Task<int> StoreActiveAsync(Guid userId, bool IsActive);


    }
}

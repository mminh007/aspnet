using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using Store.DAL.Models.Entities;

namespace Store.DAL.Repository
{
    public interface IStoreRepository
    {
        Task<int> UpdateStoreAsync(UpdateStoreModel model);

        Task<int> DeleteStoreAsync(Guid UserId);

        Task<Guid> CreateStoreAsync(RegisterStoreModel user);

        Task<StoreDTO> GetStoreInfo(Guid userId); // StoreDTO contain info Store

        Task<StoreDTO> GetStoreByKeyword(string keyword);

        Task<IEnumerable<StoreDTO>?> GetActiveStoresAsync(int page, int pageSize);
        Task<int> GetActiveStoresCountAsync();

        Task<StoreDTO?> GetStoreDetailById(Guid storeId);

        Task<int> ChangeActiveStoreAsync(Guid storeId, bool IsActive);

        Task<IEnumerable<StoreModel>> SearchStoreByKeywordAsync(string keyword);

        Task<IEnumerable<StoreModel>> SearchStoreByTagAsync(string tag);


    }
}

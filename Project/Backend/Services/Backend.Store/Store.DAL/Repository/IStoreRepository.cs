using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using Store.DAL.Models.Entities;

namespace Store.DAL.Repository
{
    public interface IStoreRepository
    {

        Task<int> DeleteStoreAsync(Guid UserId);

        Task<Guid> CreateStoreAsync(RegisterStoreModel user);

        Task<StoreModel> GetStoreInfo(Guid userId); // StoreDTO contain info Store

        Task<StoreModel> GetStoreByKeyword(string keyword);

        Task<IEnumerable<StoreDTO>?> GetActiveStoresAsync(int page, int pageSize);
        Task<int> GetActiveStoresCountAsync();

        Task<StoreModel?> GetStoreDetailById(Guid storeId);

        Task<int> ChangeActiveStoreAsync(Guid storeId, bool IsActive);

        Task<IEnumerable<StoreModel>> SearchStoreByKeywordAsync(string keyword);

        Task<IEnumerable<StoreDTO>> SearchStoreByTagPagedAsync(string tag, int page, int pageSize);
        Task<int> CountStoreByTagAsync(string tag);

        Task SaveChangesAsync();


    }
}

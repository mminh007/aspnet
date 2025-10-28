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

        Task<IEnumerable<StoreDTO>> SearchStoreByKeywordPageAsync(string keyword, int page, int pageSize);

        Task<int> CountStoreByKeywordAsync(string keyword);

        Task<IEnumerable<StoreDTO>> SearchStoreByTagPagedAsync(string tag, int page, int pageSize);
        Task<int> CountStoreByTagAsync(string tag);

        Task<int> UpsertAccountBankingAsync(Guid storeId, string bankName, string accountNumber);

        Task<int> SetAccountBalanceAsync(Guid storeId, decimal newBalance);
        Task<int> IncreaseBalanceAsync(Guid storeId, decimal amount);
        Task<int> DecreaseBalanceAsync(Guid storeId, decimal amount);
        Task<int> DeleteAccountBankingAsync(Guid storeId);

        Task SaveChangesAsync();


    }
}

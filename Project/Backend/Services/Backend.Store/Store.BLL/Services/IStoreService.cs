using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using Store.DAL.Models.Entities;

namespace Store.BLL.Services
{
    public interface IStoreService
    {
        Task<StoreResponseModel<Guid>> CreateStoreAsync(RegisterStoreModel model);

        Task<StoreResponseModel<StoreDTO>> UpdateStoreAsync(UpdateStoreModel model);

        Task<StoreResponseModel<object>> DeleteStoreAsync(Guid UserId);
        Task<StoreResponseModel<StoreDTO>> GetStoreByIdAsync(Guid UserId);

        Task<StoreResponseModel<object>> ChangeActive(ChangeActiveRequest model);

        Task<StoreResponseModel<IEnumerable<StoreDTO>>> GetActiveStoresAsync(int page, int pageSize);

        Task<StoreResponseModel<PaginatedStoreResponse>> GetActiveStoresWithPaginationAsync(int page, int pageSize);

        Task<StoreResponseModel<StoreDTO>> GetStoreDetailByIdAsync(Guid storeId);

        Task<StoreResponseModel<IEnumerable<StoreDTO>>> SearchStoreByKeywordAsync(string keyword);

        Task<StoreResponseModel<IEnumerable<StoreDTO>>> SearchStoreByTagAsync(string tag);
    }
}

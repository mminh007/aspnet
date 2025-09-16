using Store.Common.Models.Requests;
using Store.Common.Models.Responses;

namespace Store.BLL.Services
{
    public interface IStoreService
    {
        Task<StoreResponseModel<Guid>> CreateStoreAsync(RegisterStoreModel model);

        Task<StoreResponseModel<object>> UpdateStoreAsync(UpdateStoreModel model);

        Task<StoreResponseModel<object>> DeleteStoreAsync(Guid UserId);
        Task<StoreResponseModel<StoreDTO>> GetStoreByIdAsync(Guid UserId);

        Task<StoreResponseModel<object>> ChangeActive(StoreActiveModel model);

        Task<StoreResponseModel<IEnumerable<StoreDTO>>> GetAllActiveStoresAsync();

        Task<StoreResponseModel<StoreDTO>> GetStoreDetailByIdAsync(Guid storeId);
    }
}

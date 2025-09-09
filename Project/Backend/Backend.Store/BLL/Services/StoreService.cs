
using Commons.Enums;
using Commons.Models.Requests;
using Commons.Models.Responses;
using DAL.Repository;
using Microsoft.Extensions.Logging;


namespace BLL.Services
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly ILogger<StoreService> _logger;

        public StoreService(IStoreRepository storeRepository, ILogger<StoreService> logger)
        {
            _storeRepository = storeRepository;
            _logger = logger;
        }

        public async Task<StoreResponseModel<object>> ChangeActive(StoreActiveModel model)
        {
            try
            {
                var result = await _storeRepository.StoreActiveAsync(model.UserId, model.IsActive);
                if (result == 0)
                {
                    return new StoreResponseModel<object>
                    {
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Store not found"
                    };
                }

                return new StoreResponseModel<object>
                {
                    Message = OperationResult.Success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while changing store active status");
                return new StoreResponseModel<object>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Error while changing store active status"
                };
            }
        }

        public async Task<StoreResponseModel<object>> CreateStoreAsync(RegisterStoreModel model)
        {

            try
            {
                await _storeRepository.CreateStoreAsync(model);
                return new StoreResponseModel<object>
                {
                    Message = OperationResult.Success,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering new store (UserId = {UserId})", model.UserId);
                return new StoreResponseModel<object>
                {
                    Message = OperationResult.Error,
                };
            }
        }

        public async Task<StoreResponseModel<object>> DeleteStoreAsync(Guid userId)
        {
            try
            {
                await _storeRepository.DeleteStoreAsync(userId);
                return new StoreResponseModel<object>
                {
                    Message = OperationResult.Success,
                    Data = null

                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while Delete Store (User = {UserId}", userId);
                return new StoreResponseModel<object>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Error while Invisible Store"
                };
            }

        }

        public async Task<StoreResponseModel<StoreDTO>> GetStoreByIdAsync(Guid userId)
        {
            try
            {
                var storeInfo = await _storeRepository.GetStoreInfo(userId);
                if (storeInfo == null)
                {
                    return new StoreResponseModel<StoreDTO>
                    {
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Store Not Found",
                        Data = null
                    };
                }

                return new StoreResponseModel<StoreDTO>
                {
                    Message = OperationResult.Success,
                    Data = storeInfo
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Get Store Infomation");
                return new StoreResponseModel<StoreDTO>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Cannot Get Store Infomation",
                    Data = null,
                };
            }
        }

        public async Task<StoreResponseModel<object>> UpdateStoreAsync(UpdateStoreModel model)
        {
            try
            {

                await _storeRepository.UpdateStoreAsync(model);
                return new StoreResponseModel<object>
                {
                    Message = OperationResult.Success,
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating Store Infomation");
                return new StoreResponseModel<object>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Error while updating Store Infomation",
                    Data = null
                };
            }
        }

        // ✅ New: Get store detail by StoreId
        public async Task<StoreResponseModel<StoreDTO>> GetStoreDetailByIdAsync(Guid storeId)
        {
            try
            {
                var store = await _storeRepository.GetStoreDetailById(storeId);
                if (store == null)
                {
                    return new StoreResponseModel<StoreDTO>
                    {
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Store not found",
                        Data = null,
                    };
                }

                return new StoreResponseModel<StoreDTO>
                {
                    Message = OperationResult.Success,
                    Data = store
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting store detail (StoreId = {StoreId})", storeId);
                return new StoreResponseModel<StoreDTO>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Unexpected error while getting store detail"
                };
            }
        }

        // ✅ New: Get all active stores
        public async Task<StoreResponseModel<IEnumerable<StoreDTO>>> GetAllActiveStoresAsync()
        {
            try
            {
                var storesList = await _storeRepository.GetAllActiveStoresAsync();
                if (!storesList.Any())
                {
                    return new StoreResponseModel<IEnumerable<StoreDTO>>
                    {
                        Message = OperationResult.NotFound,
                        ErrorMessage = "No stores found",
                    };
                }

                return new StoreResponseModel<IEnumerable<StoreDTO>>
                {
                    Message = OperationResult.Success,
                    Data = storesList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting all active stores");
                return new StoreResponseModel<IEnumerable<StoreDTO>>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Unexpected error while getting store list"
                };
            }

        }
    }
}

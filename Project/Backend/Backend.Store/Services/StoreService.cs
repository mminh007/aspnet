
using Backend.Store.Enums;
using Backend.Store.Models;
using Backend.Store.Repository;


namespace Backend.Store.Services
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

        public async Task<StoreResponseModel> ChangeActive(StoreActiveModel model)
        {
            try
            {
                var result = await _storeRepository.StoreActiveAsync(model.UserId, model.IsActive);
                if (result == 0)
                {
                    return new StoreResponseModel
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Store not found"
                    };
                }

                return new StoreResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while changing store active status");
                return new StoreResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Error while changing store active status"
                };
            }
        }

        public async Task<StoreResponseModel> CreateStoreAsync(RegisterStoreModel model)
        {
            try
            {
                await _storeRepository.CreateStoreAsync(model);
                return new StoreResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering new store (UserId = {UserId})", model.UserId);
                return new StoreResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                };
            }
        }

        public async Task<StoreResponseModel> DeleteStoreAsync(Guid userId)
        {
            try
            {
                await _storeRepository.DeleteStoreAsync(userId);
                return new StoreResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,

                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while Delete Store (User = {UserId}", userId);
                return new StoreResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Error while Invisible Store"
                };
            }

        }

        public async Task<StoreResponseModel> GetStoreByIdAsync(Guid userId)
        {
            try
            {
                var storeInfo = await _storeRepository.GetStoreInfo(userId);
                if (storeInfo == null)
                {
                    return new StoreResponseModel
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Store Not Found"
                    };
                }

                return new StoreResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,
                    StoreInfo = storeInfo
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot Get Store Infomation");
                return new StoreResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Cannot Get Store Infomation"
                };
            }
        }

        public async Task<StoreResponseModel> UpdateStoreAsync(UpdateStoreModel model)
        {
            try
            {

                await _storeRepository.UpdateStoreAsync(model);
                return new StoreResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating Store Infomation");
                return new StoreResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Error while updating Store Infomation"
                };
            }
        }

        // ✅ New: Get store detail by StoreId
        public async Task<StoreResponseModel> GetStoreDetailByIdAsync(Guid storeId)
        {
            try
            {
                var store = await _storeRepository.GetStoreDetailById(storeId);
                if (store == null)
                {
                    return new StoreResponseModel
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Store not found"
                    };
                }

                return new StoreResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,
                    StoreInfo = store
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting store detail (StoreId = {StoreId})", storeId);
                return new StoreResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Unexpected error while getting store detail"
                };
            }
        }

        // ✅ New: Get all active stores
        public async Task<StoreResponseModel> GetAllActiveStoresAsync()
        {
            try
            {
                var stores = await _storeRepository.GetAllActiveStoresAsync();
                if (!stores.Any())
                {
                    return new StoreResponseModel
                    {
                        Success = false,
                        Message = OperationResult.NotFound,
                        ErrorMessage = "No stores found"
                    };
                }

                return new StoreResponseModel
                {
                    Success = true,
                    Message = OperationResult.Success,
                    StoreList = stores
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting all active stores");
                return new StoreResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Unexpected error while getting store list"
                };
            }

        }
    }
}

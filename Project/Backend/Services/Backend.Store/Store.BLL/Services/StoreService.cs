
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Store.Common.Configs;
using Store.Common.Enums;
using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using Store.DAL.Repository;
using System.Net.WebSockets;


namespace Store.BLL.Services
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly ILogger<StoreService> _logger;
        private readonly StaticFileConfig _staticFileConfig;

        public StoreService(IStoreRepository storeRepository, ILogger<StoreService> logger, IOptions<StaticFileConfig> staticFileConfig)
        {
            _storeRepository = storeRepository;
            _logger = logger;

            _staticFileConfig = staticFileConfig.Value;


        }

        public async Task<StoreResponseModel<object>> ChangeActive(StoreActiveModel model)
        {
            try
            {
                var result = await _storeRepository.ChangeActiveStoreAsync(model.UserId, model.IsActive);
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

        public async Task<StoreResponseModel<Guid>> CreateStoreAsync(RegisterStoreModel model)
        {

            try
            {
                var storeId = await _storeRepository.CreateStoreAsync(model);
                return new StoreResponseModel<Guid>
                {
                    Message = OperationResult.Success,
                    Data = storeId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering new store (UserId = {UserId})", model.UserId);
                return new StoreResponseModel<Guid>
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

                storeInfo.StoreImage =
                    $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{storeInfo.StoreImage}";

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

                store.StoreImage =
                    $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{store.StoreImage}";

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
        public async Task<StoreResponseModel<IEnumerable<StoreDTO>>> GetActiveStoresAsync(int page, int pageSize)
        {
            try
            {
                // Validate input parameters
                if (page < 1)
                {
                    return new StoreResponseModel<IEnumerable<StoreDTO>>
                    {
                        Message = OperationResult.Failed,
                        ErrorMessage = "Page number must be greater than 0"
                    };
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return new StoreResponseModel<IEnumerable<StoreDTO>>
                    {
                        Message = OperationResult.Failed,
                        ErrorMessage = "Page size must be between 1 and 100"
                    };
                }

                var storesList = await _storeRepository.GetActiveStoresAsync(page, pageSize);

                if (storesList == null || !storesList.Any())
                {
                    return new StoreResponseModel<IEnumerable<StoreDTO>>
                    {
                        Message = OperationResult.NotFound,
                        ErrorMessage = "No stores found for the requested page"
                    };
                }

                foreach (var store in storesList)
                {
                    store.StoreImage =
                    $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{store.StoreImage}";

                }

                return new StoreResponseModel<IEnumerable<StoreDTO>>
                {
                    Message = OperationResult.Success,
                    Data = storesList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting active stores (Page: {Page}, PageSize: {PageSize})", page, pageSize);
                return new StoreResponseModel<IEnumerable<StoreDTO>>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Unexpected error while getting store list"
                };
            }
        }

        public async Task<StoreResponseModel<PaginatedStoreResponse>> GetActiveStoresWithPaginationAsync(int page, int pageSize)
        {
            try
            {
                // Validate input parameters
                if (page < 1)
                {
                    return new StoreResponseModel<PaginatedStoreResponse>
                    {
                        Message = OperationResult.Failed,
                        ErrorMessage = "Page number must be greater than 0"
                    };
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return new StoreResponseModel<PaginatedStoreResponse>
                    {
                        Message = OperationResult.Failed,
                        ErrorMessage = "Page size must be between 1 and 100"
                    };
                }

                var storesList = await _storeRepository.GetActiveStoresAsync(page, pageSize);
                var totalRecords = await _storeRepository.GetActiveStoresCountAsync();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                if (storesList == null || !storesList.Any())
                {
                    return new StoreResponseModel<PaginatedStoreResponse>
                    {
                        Message = OperationResult.NotFound,
                        ErrorMessage = "No stores found for the requested page"
                    };
                }

                foreach (var store in storesList)
                {
                    store.StoreImage =
                    $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{store.StoreImage}";

                }

                var paginatedResponse = new PaginatedStoreResponse
                {
                    Stores = storesList,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };

                return new StoreResponseModel<PaginatedStoreResponse>
                {
                    Message = OperationResult.Success,
                    Data = paginatedResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting active stores with pagination (Page: {Page}, PageSize: {PageSize})", page, pageSize);
                return new StoreResponseModel<PaginatedStoreResponse>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Unexpected error while getting store list"
                };
            }
        }
    }
}

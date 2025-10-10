
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slugify;
using Store.BLL.Helper;
using Store.Common.Configs;
using Store.Common.Enums;
using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using Store.DAL.Models.Entities;
using Store.DAL.Repository;
using System.Net.WebSockets;


namespace Store.BLL.Services
{
    public class StoreService : IStoreService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly ILogger<StoreService> _logger;
        private readonly StaticFileConfig _staticFileConfig;
        private readonly IMapper _mapper;
        private readonly ISlugHelper _slugHelper;

        public StoreService(IStoreRepository storeRepository, ILogger<StoreService> logger, IOptions<StaticFileConfig> staticFileConfig, IMapper mapper)
        {
            _storeRepository = storeRepository;
            _logger = logger;
            _mapper = mapper;

            _staticFileConfig = staticFileConfig.Value;

            _slugHelper = new VietnameseSlugHelper();
        }

        public async Task<StoreResponseModel<object>> ChangeActive(ChangeActiveRequest model)
        {
            try
            {
                var result = await _storeRepository.ChangeActiveStoreAsync(model.StoreId, model.IsActive);
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

                var dto = _mapper.Map<StoreDTO>(storeInfo);
                return new StoreResponseModel<StoreDTO>
                {
                    Message = OperationResult.Success,
                    Data = dto
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

        public async Task<StoreResponseModel<StoreDTO>> UpdateStoreAsync(UpdateStoreModel model)
        {
            try
            {
                var store = await _storeRepository.GetStoreDetailById(model.storeId);
                if (store == null)
                {
                    return new StoreResponseModel<StoreDTO>
                    {
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Store not found"
                    };
                }

                // Nếu category thay đổi → sinh slug mới
                if (store.StoreCategory != model.StoreCategory)
                {
                    store.StoreCategorySlug = _slugHelper.GenerateSlug(model.StoreCategory);
                }

                // Cập nhật ảnh nếu có
                if (!string.IsNullOrWhiteSpace(model.StoreImage))
                {
                    store.StoreImage = model.StoreImage;
                }

                // ⚡ Map đè từ model sang entity đang tracked (AutoMapper xử lý partial update)
                _mapper.Map(model, store);

                // Cập nhật thời gian
                store.UpdatedAt = DateTime.UtcNow;

                await _storeRepository.SaveChangesAsync();

                var dto = _mapper.Map<StoreDTO>(store);
                return new StoreResponseModel<StoreDTO>
                {
                    Message = OperationResult.Success,
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating Store Infomation");
                return new StoreResponseModel<StoreDTO>
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

                var dto = _mapper.Map<StoreDTO>(store);
                return new StoreResponseModel<StoreDTO>
                {
                    Message = OperationResult.Success,
                    Data = dto,
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

        public async Task<StoreResponseModel<IEnumerable<StoreDTO>>> SearchStoreByKeywordAsync(string keyword)
        {
            var result = await _storeRepository.SearchStoreByKeywordAsync(keyword);

            var dto = _mapper.Map<IEnumerable<StoreDTO>>(result);

            return new StoreResponseModel<IEnumerable<StoreDTO>>()
            {
                Message = OperationResult.Success,
                Data = dto
            };
        }

        public async Task<StoreResponseModel<PaginatedStoreResponse>> SearchStoreByTagPagedAsync(string tagSlug, int page, int pageSize)
        {
            try
            {
                var stores = await _storeRepository.SearchStoreByTagPagedAsync(tagSlug, page, pageSize);
                var totalRecords = await _storeRepository.CountStoreByTagAsync(tagSlug);
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                if (stores == null || !stores.Any())
                {
                    return new StoreResponseModel<PaginatedStoreResponse>
                    {
                        Message = OperationResult.NotFound,
                        ErrorMessage = "Không tìm thấy cửa hàng nào."
                    };
                }

                foreach (var store in stores)
                {
                    store.StoreImage = $"{_staticFileConfig.BaseUrl}{_staticFileConfig.ImageUrl.RequestPath}/{store.StoreImage}";
                }

                var response = new PaginatedStoreResponse
                {
                    Stores = stores,
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
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while searching stores by tag with pagination (Tag={Tag})", tagSlug);
                return new StoreResponseModel<PaginatedStoreResponse>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Unexpected error while searching stores by tag"
                };
            }
        }

        private string GenerateSlug(string? categoryString)
        {
            if (string.IsNullOrWhiteSpace(categoryString)) return string.Empty;

            var helper = new SlugHelper();

            // tách nhiều tag: "Mỹ phẩm, Dược phẩm" → ["Mỹ phẩm", "Dược phẩm"]
            var tags = categoryString
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => helper.GenerateSlug(t.Trim()))
                .ToList();

            return string.Join(",", tags); // => "my-pham,duoc-pham"
        }

    }
}

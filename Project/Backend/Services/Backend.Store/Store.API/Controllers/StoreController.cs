using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.BLL.Services;
using Store.Common.Enums;
using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Store.API.Controllers
{
    [Route("api/store")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;
        private readonly ILogger<StoreController> _logger;

        public StoreController(IStoreService storeService, ILogger<StoreController> logger)
        {
            _storeService = storeService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "system")]
        public async Task<IActionResult> CreateStore([FromBody] RegisterStoreModel model)
        {
            var result = await _storeService.CreateStoreAsync(model);
            return HandleResponse(result);
        }

        [HttpGet("search-keyword")]
        public async Task<IActionResult> SearchStoreByKeyword(
            [FromQuery] string keyword,
            [FromQuery] int page,
            [FromQuery] int pageSize)
        {
            var result = await _storeService.SearchStoreByKeywordAsync(keyword, page, pageSize);
            return HandleResponse(result);
        }

        [HttpGet("search-tag")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchStoreByTag(
            [FromQuery] string tag,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 9)
        {
            if (string.IsNullOrWhiteSpace(tag))
                tag = ""; // Mặc định lấy tất cả nếu không có tag

            var result = await _storeService.SearchStoreByTagPagedAsync(tag, page, pageSize);

            //if (result.Message == OperationResult.Success)
            //{
            //    return Ok(new
            //    {
            //        statusCode = 200,
            //        message = "Operation successful",
            //        data = result.Data,
            //    });
            //}

            return HandleResponse(result);
        }


        [HttpGet("get")]
        [Authorize(Roles = "seller, system")]
        public async Task<IActionResult> GetStoreById()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var id = Guid.Parse(userId);

            var result = await _storeService.GetStoreByIdAsync(id);
            return HandleResponse(result);
        }

        [HttpGet("get-detail")]
        [Authorize(Roles = "seller, system")]
        public async Task<IActionResult> GetStoreDetail([FromQuery] Guid store_id)
        {
            var result = await _storeService.GetStoreDetailByIdAsync(store_id);
            return HandleResponse(result);
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> PublicGetStoreDetail([FromQuery] Guid store)
        {
            var result = await _storeService.GetStoreDetailByIdAsync(store);
            return HandleResponse(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> UpdateStore([FromQuery] Guid store_id, [FromBody] UpdateStoreModel model)
        {
            var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(subClaim, out var userId))
                return Unauthorized("Token không hợp lệ: sub claim bị thiếu hoặc không phải GUID");
            model.storeId = store_id;

            var result = await _storeService.UpdateStoreAsync(model);
            return HandleResponse(result);
        }

        [HttpDelete("delete")]
        [Authorize(Roles = "seller, admin")]
        public async Task<IActionResult> DeleteStore([FromQuery] Guid store_id)
        {
            var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = Guid.Parse(subClaim);

            var result = await _storeService.DeleteStoreAsync(userId);
            return HandleResponse(result);
        }

        [HttpPatch("change-active")]
        [Authorize(Roles = "seller, system")]
        public async Task<IActionResult> ChangeActive([FromBody] ChangeActiveRequest model, [FromQuery] Guid store_id)
        {
            if (User.IsInRole("seller"))
            {
                var userIdFromToken = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                                        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var UserId = Guid.Parse(userIdFromToken!);
            }

            var result = await _storeService.ChangeActive(model);
            return HandleResponse(result);
        }

        // ---------------------------
        // Buyer / Public APIs
        // ---------------------------

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoresList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Validate query parameters
            if (page < 1)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Page number must be greater than 0"
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Page size must be between 1 and 100"
                });
            }

            var result = await _storeService.GetActiveStoresAsync(page, pageSize);
            return HandleResponse(result);
        }

        [HttpGet("all-paginated")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStoresListWithPagination([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Validate query parameters
            if (page < 1)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Page number must be greater than 0"
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Page size must be between 1 and 100"
                });
            }

            var result = await _storeService.GetActiveStoresWithPaginationAsync(page, pageSize);

            if (result.Message == OperationResult.Success)
            {
                return Ok(new
                {
                    statusCode = 200,
                    message = "Operation successful",
                    data = result.Data.Stores,
                    pagination = new
                    {
                        currentPage = result.Data.CurrentPage,
                        pageSize = result.Data.PageSize,
                        totalRecords = result.Data.TotalRecords,
                        totalPages = result.Data.TotalPages,
                        hasNextPage = result.Data.HasNextPage,
                        hasPreviousPage = result.Data.HasPreviousPage
                    }
                });
            }

            return HandleResponse(new StoreResponseModel<IEnumerable<StoreDTO>>
            {
                Message = result.Message,
                ErrorMessage = result.ErrorMessage
            });
        }

        [HttpPost("settlements")]
        [Authorize(Roles = "system")]
        public async Task<IActionResult> UpdateAmountAsync([FromBody] UpdateStoreAmountRequest model)
        {
            // Lấy Idempotency-Key từ header nếu client không gửi trong body
            model.IdempotencyKey ??= Request.Headers["Idempotency-Key"].FirstOrDefault();

            // Validate cơ bản
            if (model is null)
                return BadRequest(new { statusCode = 400, message = "Body is required." });

            if (model.StoreId == Guid.Empty)
                return BadRequest(new { statusCode = 400, message = "StoreId is required." });

            if (model.Amount <= 0)
                return BadRequest(new { statusCode = 400, message = "Amount must be greater than 0." });

            // Gọi service xử lý settle/credit
            var result = await _storeService.SettleAsync(model);

            return HandleResponse(result);
        }


        private IActionResult HandleResponse<T>(StoreResponseModel<T> response)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(new
                {
                    statusCode = 200,
                    message = "Operation successful",
                    data = response.Data
                }),

                OperationResult.Failed => BadRequest(new
                {
                    statusCode = 400,
                    message = response.ErrorMessage ?? "Operation failed"
                }),

                OperationResult.Forbidden => StatusCode(403, new
                {
                    statusCode = 403,
                    message = response.ErrorMessage ?? "Forbidden - You don't have permission"
                }),

                OperationResult.NotFound => NotFound(new
                {
                    statusCode = 404,
                    message = response.ErrorMessage ?? "Not found"
                }),

                OperationResult.Conflict => Conflict(new
                {
                    statusCode = 409,
                    message = response.ErrorMessage ?? "Conflict occurred"
                }),

                OperationResult.Error => StatusCode(500, new
                {
                    statusCode = 500,
                    message = response.ErrorMessage ?? "Unexpected error!"
                }),

                _ => StatusCode(500, new { statusCode = 500, message = "Unexpected error!" })
            };
        }

    }
}

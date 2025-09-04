using Backend.Store.Enums;
using Backend.Store.Models;
using Backend.Store.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Store.Controllers
{
    [Route("api/[controller]")]
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

            return result.Message switch
            {
                OperationResult.Success => Ok(new { message = "Register Successfully!" }),
                OperationResult.Failed => BadRequest(new { message = "Registration failed" }),
                OperationResult.NotFound => NotFound(new { message = "Store service unavailable" }),
                _ => StatusCode(500, new { message = "Unexpected error!" })
            };

        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "seller, system")]
        public async Task<IActionResult> GetStoreById(Guid userId)
        {
            var result = await _storeService.GetStoreByIdAsync(userId);

            return result.Message switch
            {
                OperationResult.Success => Ok(result.StoreInfo),
                OperationResult.NotFound => NotFound(new { message = "Store not found" }),
                _ => StatusCode(500, new { message = result.ErrorMessage ?? "Unexpected error!" })
            };
        }

        [HttpPut]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> UpdateStore([FromBody] UpdateStoreModel model)
        {
            var result = await _storeService.UpdateStoreAsync(model);

            return result.Message switch
            {
                OperationResult.Success => Ok(new { message = "Update Successfully!" }),
                OperationResult.NotFound => NotFound(new { message = "Store not found" }),
                OperationResult.Failed => BadRequest(new { message = "Update failed" }),
                _ => StatusCode(500, new { message = result.ErrorMessage ?? "Unexpected error!" })
            };
        }

        [HttpDelete("{userId:guid}")] // Optional: Send Event to Product, Order to IsActive = false;
        [Authorize(Roles = "seller, admin")]
        public async Task<IActionResult> DeleteStore(Guid userId)
        {
            var result = await _storeService.DeleteStoreAsync(userId);

            return result.Message switch
            {
                OperationResult.Success => Ok(new { message = "Delete Successfully!" }),
                OperationResult.NotFound => NotFound(new { message = "Store not found" }),
                OperationResult.Failed => BadRequest(new { message = "Delete failed" }),
                _ => StatusCode(500, new { message = result.ErrorMessage ?? "Unexpected error!" })
            };
        }

        [HttpPatch("change-active")]
        [Authorize(Roles = "seller, system")]
        public async Task<IActionResult> ChangeActive([FromBody] StoreActiveModel model)
        {
            // Nếu là seller thì ép UserId từ JWT claim
            if (User.IsInRole("seller"))
            {
                var userIdFromToken = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                model.UserId = Guid.Parse(userIdFromToken!);
            }

            var result = await _storeService.ChangeActive(model);

            return result.Message switch
            {
                OperationResult.Success => Ok(new { message = "Store active status updated!" }),
                OperationResult.NotFound => NotFound(new { message = "Store not found" }),
                _ => StatusCode(500, new { message = result.ErrorMessage ?? "Unexpected error!" })
            };
        }

        // ---------------------------
        // Buyer / Public APIs
        // ---------------------------

        /// <summary>
        /// Lấy danh sách tất cả store đang hoạt động
        /// </summary>
        [HttpGet("all")]
        [AllowAnonymous] // hoặc [Authorize(Roles = "buyer,seller,admin")]
        public async Task<IActionResult> GetAllStores()
        {
            var result = await _storeService.GetAllActiveStoresAsync();

            return result.Message switch
            {
                OperationResult.Success => Ok(result.StoreList),
                OperationResult.NotFound => NotFound(new { message = "No stores found" }),
                _ => StatusCode(500, new { message = result.ErrorMessage ?? "Unexpected error!" })
            };
        }

        /// <summary>
        /// Lấy thông tin store theo StoreId
        /// </summary>
        [HttpGet("detail/{storeId:guid}")]
        [AllowAnonymous] // hoặc [Authorize(Roles = "buyer,seller,admin")]
        public async Task<IActionResult> GetStoreDetail(Guid storeId)
        {
            var result = await _storeService.GetStoreDetailByIdAsync(storeId);

            return result.Message switch
            {
                OperationResult.Success => Ok(result.StoreInfo),
                OperationResult.NotFound => NotFound(new { message = "Store not found" }),
                _ => StatusCode(500, new { message = result.ErrorMessage ?? "Unexpected error!" })
            };
        }



    }
}

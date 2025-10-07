using Adminstrator.Models.Stores;
using Adminstrator.Services.Interfaces;
using DotNetEnv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adminstrator.Controllers
{
    [Route("admin/[controller]/[action]")]
    public class StoreController : Controller
    {
        private readonly ILogger<StoreController> _logger;
        private readonly IStoreService _storeService;

        public StoreController(ILogger<StoreController> logger, IStoreService storeService)
        {
            _logger = logger;
            _storeService = storeService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var (message, statusCode, data) = await _storeService.GetStoreByUserIdAsync();

            if (data == null)
            {
                TempData["Error"] = GetMessageByStatusCode(statusCode, message);
                return RedirectToAction("Index");
            }

            if (data.StoreId == Guid.Empty)
            {
                TempData["Error"] = "Store not found for the given user ID.";
  
            }

            _logger.LogInformation("✅ Retrieved store for: StoreId={StoreId}", data.StoreId);

            return View(data);
        }


        [HttpPut]
        public async Task<IActionResult> Update([FromQuery] Guid storeId, UpdateStoreModel model, IFormFile? StoreImage)
        {
            try
            {
                // Nếu có file upload
                if (StoreImage != null && StoreImage.Length > 0)
                {
                    // 🔹 Chỉ lấy tên file, bỏ hết path
                    var fileName = Path.GetFileName(StoreImage.FileName);
                    model.StoreImage = $"{model.StoreId}/{StoreImage.FileName}";
                    ;
                }

                // 🔹 Gọi service cập nhật
                var result = await _storeService.UpdateStoreAsync(model);

                if (result.statusCode != 200)
                {
                    return StatusCode(result.statusCode, new
                    {
                        success = false,
                        message = result.message
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật thành công",
                    data = result.data
                });
            }
            catch (Exception ex)
            {
                // Luôn trả JSON kể cả khi lỗi
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi máy chủ: " + ex.Message
                });
            }
        }



        [HttpPatch]
        public async Task<IActionResult> ChangeActive([FromBody] ChangeActiveRequest request)
        {
            var result = await _storeService.ChangeActiveStoreAsync(request);

            if (result.statusCode == 200)
                return Ok(new { success = true, message = "Cập nhật thành công" });

            return BadRequest(new { success = false, message = result.message });
        }

        /// <summary>
        /// Helper method: gán message khác nhau dựa trên statusCode
        /// </summary>
        private string GetMessageByStatusCode(int statusCode, string? message)
        {
            return statusCode switch
            {
                200 => message ?? "Operation successful!",
                400 => message ?? "Invalid request. Please try again.",
                401 => "Unauthorized. Please login again.",
                403 => "Forbidden. You don’t have permission to access this resource.",
                404 => message ?? "Store not found for the given user ID.",
                409 => message ?? "Conflict occurred. Resource already exists.",
                500 => message ?? "Internal server error. Please contact administrator.",
                _ => message ?? "Unexpected error occurred."
            };
        }
    }
}

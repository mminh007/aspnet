using Adminstrator.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adminstrator.Controllers
{
    [Route("admin/[controller]/[action]")]
    public class StoreController : Controller
    {
        private readonly ILogger<StoreController> _logger;
        private readonly IStoreServices _storeService;

        public StoreController(ILogger<StoreController> logger, IStoreServices storeService)
        {
            _logger = logger;
            _storeService = storeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Management(string id)
        {
            var isValidGuid = Guid.Parse(id);
            var (message, statusCode, data) = await _storeService.GetStoreByUserIdAsync(isValidGuid);

            if (data == null)
            {
                TempData["Error"] = GetMessageByStatusCode(statusCode, message);
                return RedirectToAction("Index");
            }

            if (data.StoreId == Guid.Empty)
            {
                _logger.LogWarning("⚠️ StoreId is empty for userId={UserId}", id);
                TempData["Error"] = "Store not found for the given user ID.";
  
            }

            _logger.LogInformation("✅ Retrieved store for userId={UserId}: StoreId={StoreId}", id, data.StoreId);

            return View(data);
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

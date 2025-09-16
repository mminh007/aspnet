using Frontend.HttpsClients.Stores;
using Frontend.Models;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStoreService _storeService;

        public HomeController(IStoreService storeService, ILogger<HomeController> logger)
        {
            _storeService = storeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var results = await _storeService.GetAllStoresActiveAsync();

            return View(results);
        }

        [Route("Store/{storeId}")]
        public async Task<IActionResult> StoreDetail(Guid storeId)
        {
            _logger.LogInformation("✅ Retrieved store for StoreId={StoreId}", storeId);

            if (storeId == Guid.Empty)
            {
                _logger.LogWarning("⚠️ storeId EMPTY khi gọi Store");
                return BadRequest("storeId is required");
            }

            var (message, statusCode, store) = await _storeService.GetStoresDetailAsync(storeId);
            if (store == null)
            {
                TempData["ErrorMessage"] = $"Error {statusCode}: {message}";
                return RedirectToAction("Index");
            }
            return View(store);
        }


    }
}

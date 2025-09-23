using Frontend.HttpsClients.Stores;
using Frontend.Models.Stores;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
            var (message, statusCode, data) = await _storeService.GetStoresPagedAsync(1, 9);
            if (statusCode != 200)
            {
                _logger.LogWarning("⚠️ Failed to retrieve active stores: {Message}", message);
                ViewBag.Message = $"Error {statusCode}: {message}";
                return View(Enumerable.Empty<StoreDto>());
            }

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreStores(int page, int pageSize = 9)
        {
            var (message, statusCode, data) = await _storeService.GetStoresPagedAsync(page, pageSize);
            if (statusCode != 200 || data == null)
            {
                return Json(new { success = false, message });
            }

            return PartialView("_StoreCardPartial", data); // trả về HTML partial
        }
    }
}

using Frontend.Helpers;
using Frontend.HttpsClients.Stores;
using Frontend.Models.Stores;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Slugify;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStoreService _storeService;
        private readonly ISlugHelper _slugHelper;


        public HomeController(IStoreService storeService, ILogger<HomeController> logger)
        {
            _storeService = storeService;
            _logger = logger;
            _slugHelper = new VietnameseSlugHelper();
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

        [HttpGet]
        public async Task<IActionResult> SearchStoreByTag(string tag, int page = 1, int pageSize = 9)
        {
            // ✅ Nếu tag null hoặc rỗng thì để trống
            string slugTag = string.Empty;

            if (!string.IsNullOrWhiteSpace(tag))
            {

                // "Đồ ăn" → "do-an", "Mỹ phẩm, Dược phẩm" → "my-pham-duoc-pham"
                slugTag = _slugHelper.GenerateSlug(tag);
            }

            var (message, statusCode, data) = await _storeService.GetStoresByTagPagedAsync(slugTag, page, pageSize);

            ViewData["SelectedTag"] = tag;

            if (data == null)
                return View("SearchStoreByTag", new PaginatedStoreResponse { Stores = new List<StoreDto>() });

            return View("SearchStoreByTag", data);
        }


        [HttpGet("Home/search")]
        public async Task<IActionResult> SearchStoreByKeyword([FromQuery] string q, int page, int pageSize)
        {
            // ✅ Nếu tag null hoặc rỗng thì để trống
            string slugTag = string.Empty;

            if (!string.IsNullOrWhiteSpace(q))
            {

                // "Đồ ăn" → "do-an", "Mỹ phẩm, Dược phẩm" → "my-pham-duoc-pham"
                slugTag = _slugHelper.GenerateSlug(q);
            }

            var (message, statusCode, data) = await _storeService.GetStoreByKeywordAsync(slugTag, page, pageSize);


            ViewData["SelectedKeyword"] = q;

            if (data == null)
                return View("SearchStoreByKeyword", new PaginatedStoreResponse { Stores = new List<StoreDto>() });

            return View("SearchStoreByKeyword", data);
        }
    }
}

using Frontend.HttpsClients.Products;
using Frontend.HttpsClients.Stores;
using Frontend.Models;
using Frontend.Models.Products;
using Frontend.Models.Stores;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;

        public HomeController(IStoreService storeService, IOrderService orderService, ILogger<HomeController> logger)
        {
            _storeService = storeService;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(Guid id)
        {   
            if(id == Guid.Empty)
            {
                _logger.LogWarning("⚠️ userId EMPTY");
                TempData["CountItemsInCart"] = 0;
                ViewBag.CountItems = TempData["CountItemsInCart"];

                //var tempCount = TempData["CountItemsInCart"];
                //_logger.LogInformation($"TempData['CountItemsInCart']: {tempCount}");
            }
            else
            {
                var (messageOrder, statusCodeOrder, dataOrder) = await _orderService.CountingItemsInCart(id);
                if (statusCodeOrder != 200)
                {
                    _logger.LogWarning("⚠️ Failed to retrieve cart item count: {Message}", messageOrder);
                    ViewBag.Message = $"Error {statusCodeOrder}: {messageOrder}";
                    TempData["CountItemsInCart"] = 0;
                    ViewBag.CountItems = TempData["CountItemsInCart"];

                    //var tempCount = TempData["CountItemsInCart"];
                    //_logger.LogInformation($"TempData['CountItemsInCart']: {tempCount}");
                }
                else
                {
                    TempData["CountItemsInCart"] = dataOrder?.CountItems ?? 0;
                    ViewBag.CountItems = TempData["CountItemsInCart"];

                    //var tempCount = TempData["CountItemsInCart"];
                    //_logger.LogInformation($"TempData['CountItemsInCart']: {tempCount}");
                }
            }

            

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

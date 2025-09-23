using static Frontend.Models.Products.DTOs;
using static Frontend.Models.Orders.DTOs;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models.Orders.Requests;

namespace Frontend.Controllers
{
    public class ProductController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IStoreService storeService, IProductService productService, IOrderService orderService,
                                 ILogger<ProductController> logger)
        {
            _storeService = storeService;
            _productService = productService;
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] Guid storeId )
        {
            var tempCount = TempData["CountItemsInCart"];
            _logger.LogInformation($"From ProductController => TempData['CountItemsInCart']: {tempCount}");
            _logger.LogInformation("✅ Retrieved store for StoreId={StoreId}", storeId);

            if (storeId == Guid.Empty)
            {
                _logger.LogWarning("⚠️ storeId EMPTY khi gọi Store");
                return BadRequest("storeId is required");
            }

            var (message, statusCode, store) = await _storeService.GetStoresDetailAsync(storeId);

            var (prodMessage, prodStatusCode, products) = await _productService.GetProductByStoreIdAsync(storeId);

            var (storeCategoriesMessage, storeCategoriesStatusCode, categories) = await _productService.SearchCategoriesByStoreIdAsync(storeId);

            if (store == null)
            {
                _logger.LogWarning("⚠️ Store is NULL for storeId={StoreId}", storeId);
                ViewBag.Message = $"Error {statusCode}: {message}";
            }


            if (products == null)
            {
                _logger.LogWarning("⚠️ Products are NULL for storeId={StoreId}", storeId);
                ViewBag.prodMessage = $"No products found for this store.";
            }


            if (categories == null)
            {
                _logger.LogWarning("⚠️ Categories are NULL for storeId={StoreId}", storeId);
                ViewBag.catMessage = $"No categories found for this store.";
            }

            var model = (Store: store,
                         Categories: categories?.ToList() ?? new List<CategoryDTO>(),
                         Products: products?.ToList() ?? new List<ProductBuyerDTO>());


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToCart(Guid buyer, [FromBody] RequestItemsToCartModel dto)
        {
            if (dto.ProductId == Guid.Empty || buyer == Guid.Empty)
            {
                return BadRequest(new { message = "ProductId and UserId are required." });
            }

            var (msg, statusCode, countItems) = await _orderService.AddProductToCart(buyer, dto);
            _logger.LogInformation("Product {ProductId} added to cart for User {UserId}", dto.ProductId, buyer);

            TempData["Message"] = msg;
            TempData["CountItemsInCart"] = countItems;

            return Ok(new
            {
                message = msg,
                countItems = countItems
            });
        }
    }
}

using Frontend.Models.Products;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class ProductController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IStoreService storeService, IProductService productService, ILogger<ProductController> logger)
        {
            _storeService = storeService;
            _productService = productService;
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
                         Categories: categories?.ToList() ?? new List<DTOs.CategoryDTO>(),
                         Products: products?.ToList() ?? new List<DTOs.ProductBuyerDTO>());


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToCart(Guid productId, Guid userId)
        {
            if (productId == Guid.Empty || userId == Guid.Empty)
            {
                return BadRequest("ProductId and UserId are required.");
            }
            // Logic to add product to cart would go here.
            // This is a placeholder for demonstration purposes.
            _logger.LogInformation("Product {ProductId} added to cart for User {UserId}", productId, userId);
            TempData["Message"] = "Product added to cart successfully!";
            return View();


    }
}

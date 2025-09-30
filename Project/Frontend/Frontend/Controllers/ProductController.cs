using static Frontend.Models.Products.DTOs;
using static Frontend.Models.Orders.DTOs;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models.Orders.Requests;
using Frontend.Models.Auth;

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
        public async Task<IActionResult> Index([FromQuery] Guid store, Guid user )
        {
            var tempCount = TempData["CountItemsInCart"];
            _logger.LogInformation($"From ProductController => TempData['CountItemsInCart']: {tempCount}");
            _logger.LogInformation("✅ Retrieved store for StoreId={StoreId}", store);

            if (store == Guid.Empty)
            {
                _logger.LogWarning("⚠️ storeId EMPTY khi gọi Store");
                return BadRequest("storeId is required");
            }

            var (message, statusCode, storeData) = await _storeService.GetStoresDetailAsync(store);

            var (prodMessage, prodStatusCode, productsData) = await _productService.GetProductByStoreIdAsync(store);

            var (storeCategoriesMessage, storeCategoriesStatusCode, categoriesData) = await _productService.SearchCategoriesByStoreIdAsync(store);

            var (cartMessage, cartStatus, cartItems) = await _orderService.GetCartInStore(user, store);

            if (storeData == null)
            {
                _logger.LogWarning("⚠️ Store is NULL for storeId={StoreId}", store);
                ViewBag.Message = $"Error {statusCode}: {message}";
            }


            if (productsData == null)
            {
                _logger.LogWarning("⚠️ Products are NULL for storeId={StoreId}", store);
                ViewBag.prodMessage = $"No products found for this store.";
            }


            if (categoriesData == null)
            {
                _logger.LogWarning("⚠️ Categories are NULL for storeId={StoreId}", store);
                ViewBag.catMessage = $"No categories found for this store.";
            }

            //_logger.LogInformation("ProductImage: {image}", orderData);


            //foreach (var o in orderData)
            //{
            //    _logger.LogInformation("CartItem in store", o);
            //}

            var model = (Store: storeData,
                         Categories: categoriesData?.ToList() ?? new List<CategoryDTO>(),
                         Products: productsData?.ToList() ?? new List<ProductBuyerDTO>(),
                         CartItems: cartItems ?? Enumerable.Empty<CartItemDTO>());


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToCart(Guid buyer, [FromBody] RequestItemsToCartModel dto)
        {
            if (dto.ProductId == Guid.Empty || buyer == Guid.Empty)
            {
                return BadRequest(new { message = "ProductId and UserId are required." });
            }

            var (msg, statusCode, countItems, cartStore) = await _orderService.AddProductToCart(buyer, dto);

            var (cartMsg, cartStatus, cartItems) = await _orderService.GetCartInStore(buyer, dto.StoreId);
            var totalItemsInStore = cartItems?.Sum(i => i.Quantity) ?? 0;

            _logger.LogInformation("Product {ProductId} added to cart for User {UserId}", dto.ProductId, buyer);

            TempData["Message"] = msg;
            TempData["CountItemsInCart"] = countItems;

            return Ok(new
            {
                message = msg,
                countItems = countItems,           
                cartStore = cartStore,              
                countItemsInStore = totalItemsInStore  
            });
        }
    }
}

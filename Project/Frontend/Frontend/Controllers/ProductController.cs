using Frontend.Models.Auth;
using Frontend.Models.Orders.Requests;
using Frontend.Models.Stores;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static Frontend.Models.Orders.DTOs;
using static Frontend.Models.Products.DTOs;

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

            var (countMsg, countStatus, countData) = await _orderService.CountingItemsInCart(user);
            int totalCartItems = countData?.CountItems ?? 0;

            var model = (Store: storeData,
                         Categories: categoriesData?.ToList() ?? new List<CategoryDTO>(),
                         Products: productsData?.ToList() ?? new List<ProductBuyerDTO>(),
                         CartItems: cartItems ?? Enumerable.Empty<CartItemDTO>(),
                         TotalCartItems: totalCartItems);

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> AddProductToCart(Guid buyer, [FromBody] RequestItemsToCartModel dto)
        {
            if (dto.ProductId == Guid.Empty || buyer == Guid.Empty)
            {
                return BadRequest(new { message = "ProductId and UserId are required." });
            }

            var (msg, statusCode, countItems, cart) = await _orderService.AddProductToCart(buyer, dto);

            var itemInStore = cart.Items.Where(i => i.StoreId == dto.StoreId).ToList();

            var totalItemsInStore = itemInStore.Sum(i => i.Quantity);

            //var (cartMsg, cartStatus, cartItems) = await _orderService.GetCartInStore(buyer, dto.StoreId);
            //var totalItemsInStore = cartItems?.Sum(i => i.Quantity) ?? 0;

            _logger.LogInformation("Product {ProductId} added to cart for User {UserId}", dto.ProductId, buyer);

            TempData["Message"] = msg;
            TempData["CountItemsInCart"] = countItems;

            return Ok(new
            {
                message = msg,
                countItems = countItems,           
                cartStore = cart,              
                countItemsInStore = totalItemsInStore  
            });
        }
    }
}

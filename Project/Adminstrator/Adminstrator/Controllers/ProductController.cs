using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Products;
using Adminstrator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Adminstrator.Controllers
{
    public class ProductController : Controller
    {
        private readonly IStoreServices _storeService;
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IStoreApiClient storeApiClient,
            IProductService productService,
            IStoreServices storeService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _storeService = storeService;
            _logger = logger;
        }

        [Route("Product/Management/{storeId:guid}")]
        public async Task<IActionResult> Management(Guid storeId)
        {

            _logger.LogInformation("✅ Retrieved store for StoreId={StoreId}", storeId);

            if (storeId == Guid.Empty)
            {
                _logger.LogWarning("⚠️ storeId EMPTY khi gọi Product/Management");
                return BadRequest("storeId is required");
            }

            // 1. Call StoreServices to get store by id
            var (msgStore, codeStore, store) = await _storeService.GetStoreByIdAsync(storeId);

            // 2. Lấy products của store
            var (okProd, msgProd, codeProd, products) = await _productService.GetByStoreAsync(storeId);

            // 3. Lấy categories của store
            var (okCate, msgCate, codeCate, categories) = await _productService.SearchCategoriesAsync(storeId);

            if (store == null)
                _logger.LogWarning("⚠️ Store is NULL for storeId={StoreId}", storeId);

            if (products == null)
                _logger.LogWarning("⚠️ Products are NULL for storeId={StoreId}", storeId);

            if (categories == null)
                _logger.LogWarning("⚠️ Categories are NULL for storeId={StoreId}", storeId);

            var model = (Store: store,
                         Categories: categories?.ToList() ?? new List<DTOs.CategoryDTO>(),
                         Products: products?.ToList() ?? new List<DTOs.ProductSellerDTO>());

            return View(model);
        }

    }
}

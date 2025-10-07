using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Products;
using Adminstrator.Models.Products.Requests;
using Adminstrator.Models.Stores;
using Adminstrator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Sprache;

namespace Adminstrator.Controllers
{
    [Route("Product")]
    public class ProductController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            IStoreService storeService,
            ILogger<ProductController> logger)
        {
            _productService = productService;
            _storeService = storeService;
            _logger = logger;
        }

        [Route("Management/{storeId:guid}")]
        public async Task<IActionResult> Index(Guid storeId)
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

        [HttpPost("update-product/{storeId}")]
        public async Task<IActionResult> UpdateProduct(Guid storeId, UpdateProductModel model, IFormFile? ProductImageFile)
        {
            model.Quantity = 99;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);    
            }

            if (ProductImageFile != null && ProductImageFile.Length > 0)
            {
                // 🔹 Chỉ lấy tên file, bỏ hết path
                var fileName = Path.GetFileName(ProductImageFile.FileName);
                model.ProductImage = $"{storeId}/{ProductImageFile.FileName}";
            }


            var result = await _productService.Update(model.ProductId, model);

            return RedirectToAction("Index", new { storeId });
        }

        [HttpPost("create-product")]
        public async Task<IActionResult> CreateProduct(DTOs.ProductDTO model, IFormFile? NewImageFile)
        {
            model.Quantity = 99;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (NewImageFile != null && NewImageFile.Length > 0)
            {
                // 🔹 Chỉ lấy tên file, bỏ hết path
                var fileName = Path.GetFileName(NewImageFile.FileName);
                model.ProductImage = $"{model.StoreId}/{NewImageFile.FileName}";
            }

            var result = await _productService.Create(model);

            return RedirectToAction("Index", new { model.StoreId });
        }

        [HttpDelete("delete-product")]
        public async Task<IActionResult> DeleteProduct(Guid productId, Guid storeId)
        {
            var result = await _productService.Delete(productId);

            return Json(new
            {
                success = result.Success,
                message = result.Message ?? "Đã xóa sản phẩm thành công."
            });
            //return RedirectToAction("Index", new { storeId });
        }

        [HttpPut("change-active")]
        public async Task<IActionResult> ChangeActive([FromBody] ChangeActiveProduct request)
        {
            var result = await _productService.ChangeActiveProduct(request);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        [HttpPost("create-category")]
        public async Task<IActionResult> CreateCategory([FromBody] DTOs.CategoryDTO request)
        {
            var result = await _productService.CreateCategoryAsync(request);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }

        [HttpDelete("delete-category")]
        public async Task<IActionResult> DeleteCategory([FromQuery] string category_id)
        {
            var categoryId = Guid.Parse(category_id);
            var result = await _productService.DeleteCategoryAsync(categoryId);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
    }
}

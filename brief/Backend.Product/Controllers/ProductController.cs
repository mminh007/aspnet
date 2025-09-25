using Backend.Product.Enums;
using Backend.Product.Services;
using Backend.Product.Models.Responses;
using Backend.Product.Models.Requests;
using Backend.Product.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AutoMapper;

namespace Backend.Product.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Seller,Admin")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        

        public ProductController(IProductService service)
        {
            _service = service;
        }

        // ---------------------------
        // Product APIs
        // ---------------------------

  
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(string id)
        {
            var productId = Guid.Parse(id);
            var response = await _service.GetProductByIdAsync(productId);
            return HandleResponse(response);
        }

 
        [HttpGet("store/{storeId}")]
        public async Task<IActionResult> GetProductsByStore(string storeId)
        {
            var id = Guid.Parse(storeId);
            var response = await _service.GetProductsByStoreAsync(id, User);
            return HandleResponse(response);
        }

 
        [HttpGet("store/{storeId}/category")]
        public async Task<IActionResult> GetProductsByStoreAndCategory(string storeId, string categoryId)
        {
            var StoreId = Guid.Parse(storeId);
            var CateId = Guid.Parse(categoryId);

            var response = await _service.GetProductsByStoreAndCategoryAsync(StoreId, CateId);
            return HandleResponse(response);
        }


        [HttpGet("store/{storeId}/search")]
        public async Task<IActionResult> SearchProductsByStore(string storeId, [FromQuery] string keyword)
        {
            var StoreId = Guid.Parse(storeId);
            var response = await _service.SearchProductsByStoreAsync(StoreId, keyword);
            return HandleResponse(response);
        }

    
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] DTOs.ProductDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _service.CreateProductAsync(dto);
            return HandleResponse(response);
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateProduct([FromBody] IEnumerable<UpdateProductModel> dto)
        {
            var response = await _service.UpdateProductAsync(dto);
            return HandleResponse(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var response = await _service.DeleteProductAsync(id);
            return HandleResponse(response);
        }
       
        // ---------------------------
        // Category APIs
        // ---------------------------


        [HttpPost("category")]
        public async Task<IActionResult> CreateCategory([FromBody] DTOs.CategoryDTO category)
        {
            var response = await _service.CreateCategoryAsync(category);
            return HandleResponse(response);
        }

        [HttpGet("category/search")]
        public async Task<IActionResult> SearchCategories([FromQuery] string storeId)
        {
            var StoreId = Guid.Parse(storeId);
            var response = await _service.SearchCategoriesAsync(StoreId);
            return HandleResponse(response);
        }

        // ---------------------------
        // Helper để map OperationResult -> HTTP Status Code
        // ---------------------------
        private IActionResult HandleResponse(ProductResponseModel response)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(response),
                OperationResult.NotFound => NotFound(response),
                OperationResult.Failed => BadRequest(response),
                OperationResult.Error => StatusCode(500, response),
                _ => StatusCode(500, response)
            };
        }
    }
}

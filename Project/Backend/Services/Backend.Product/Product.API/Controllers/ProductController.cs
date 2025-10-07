using AutoMapper;
using BLL.Services.Interfaces;
using Common.Enums;
using Common.Models;
using Common.Models.Requests;
using Common.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Common.Models.Requests;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
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
        

        [HttpGet("search/{id:guid}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            //var productId = Guid.Parse(id);
            var response = await _service.GetProductByIdAsync(id);
            return HandleResponse(response);
        }


        [HttpGet("get-product/{storeId:guid}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> GetProductsByStore(Guid storeId)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            //var id = Guid.Parse(storeId);
            var response = await _service.GetProductsByStoreAsync(storeId, userRole);
            return HandleResponse(response);
        }


        [HttpGet("get/{storeId:guid}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> GetProductsByStoreAndCategory(Guid storeId, [FromQuery] Guid category_id)
        {
            //var StoreId = Guid.Parse(storeId);
            //var CateId = Guid.Parse(categoryId);

            var response = await _service.GetProductsByStoreAndCategoryAsync(storeId, category_id);
            return HandleResponse(response);
        }


        [HttpGet("search-product/{storeId:guid}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> SearchProductsByStore(Guid storeId, [FromQuery] string keyword)
        {
            //var StoreId = Guid.Parse(storeId);
            var response = await _service.SearchProductsByStoreAsync(storeId, keyword);
            return HandleResponse(response);
        }


        [HttpPost("create-product")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> CreateProduct([FromBody] DTOs.ProductDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _service.CreateProductAsync(dto);
            return HandleResponse(response);
        }


        [HttpPost("update/{productId:guid}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductModel dto, Guid productId)
        {
            var response = await _service.UpdateProductAsync(dto);
            return HandleResponse(response);
        }

        [HttpPut("update-active")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> UpdateProductActive([FromBody] ChangeActiveProduct request)
        {
            var result = await _service.ChangeActiveProductAsync(request);

            return HandleResponse(result);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var response = await _service.DeleteProductAsync(id);
            return HandleResponse(response);
        }



        // ---------------------------
        // Category APIs
        // ---------------------------


        [HttpPost("create-category")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> CreateCategory([FromBody] DTOs.CategoryDTO category)
        {
            var response = await _service.CreateCategoryAsync(category);
            return HandleResponse(response);
        }

        [HttpGet("search-category/{storeId:guid}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> SearchCategories(Guid storeId)
        {
            //var StoreId = Guid.Parse(storeId);
            var response = await _service.SearchCategoriesAsync(storeId);
            return HandleResponse(response);
        }

        [HttpDelete("delete-category/{categoryId}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> DeleteCategory(Guid categoryId)
        {
            var result = await _service.DeleteCategoryAsync(categoryId);

            return HandleResponse(result);
        }

        // ---------------------------
        // Helper để map OperationResult -> HTTP Status Code
        // ---------------------------
        private IActionResult HandleResponse<T>(ProductResponseModel<T> response)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(new
                {
                    statusCode = 200,
                    message = "Operation successful",
                    data = response.Data
                }),

                OperationResult.Failed => BadRequest(new
                {
                    statusCode = 400,
                    message = response.ErrorMessage ?? "Operation failed"
                }),

                OperationResult.Forbidden => StatusCode(403, new
                {
                    statusCode = 403,
                    message = response.ErrorMessage ?? "Forbidden - You don't have permission"
                }),

                OperationResult.NotFound => NotFound(new
                {
                    statusCode = 404,
                    message = response.ErrorMessage ?? "Not found"
                }),

                OperationResult.Conflict => Conflict(new
                {
                    statusCode = 409,
                    message = response.ErrorMessage ?? "Conflict occurred"
                }),

                OperationResult.Error => StatusCode(500, new
                {
                    statusCode = 500,
                    message = response.ErrorMessage ?? "Unexpected error!"
                }),

                _ => StatusCode(500, new { statusCode = 500, message = "Unexpected error!" })
            };
        }

    }
}


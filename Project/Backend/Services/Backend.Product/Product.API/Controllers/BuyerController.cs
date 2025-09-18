using BLL.Services.Interfaces;
using Common.Enums;
using Common.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyerProductController : ControllerBase
    {
        private readonly IProductService _service;

        public BuyerProductController(IProductService service)
        {
            _service = service;
        }


        //[HttpGet("category/{categoryId}")]
        //[AllowAnonymous] // hoặc [Authorize(Roles = "Buyer,Seller,Admin")]
        //public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        //{
        //    var response = await _service.GetProductsByCategoryForBuyerAsync(categoryId);
        //    return HandleResponse(response);
        //}

        // Get Price for Order Service
        [HttpPost("order/prices")]   // 18/9
        public async Task<IActionResult> OrderGetProductInfo2([FromBody] List<Guid> productIds)
        {
            var response = await _service.OrderGetProductInfo2(productIds);
            return HandleResponse(response);
        }

        [HttpGet("search/store")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchProducts([FromQuery] Guid store)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            //var id = Guid.Parse(storeId);
            var response = await _service.GetProductsByStoreAsync(store, userRole);
            return HandleResponse(response);
        }

        [HttpGet("search/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchCategories([FromQuery] Guid store)
        {
            //var StoreId = Guid.Parse(storeId);
            var response = await _service.SearchCategoriesAsync(storeId);
            return HandleResponse(response);
        }

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





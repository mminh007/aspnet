using Common.Enums;
using BLL.Services.Interfaces;
using Common.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpGet("search/store/{storeId}")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchProducts(string StoreId)
        {
            var id = Guid.Parse(StoreId);
            var response = await _service.GetProductsByStoreAsync(id, User);
            return HandleResponse(response);
        }

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




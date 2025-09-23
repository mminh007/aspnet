using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.BLL.Services;
using Order.Common.Enums;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "buyer, system")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Lấy giỏ hàng của user
        /// </summary>
        [HttpGet("get-cart")]
        public async Task<IActionResult> GetCart([FromQuery] Guid buyer)
        {
            var result = await _cartService.GetCartAsync(buyer, "check");
            return HandleResponse(result);
        }

        [HttpGet("counting-item")]
        public async Task<IActionResult> GetCountItems([FromQuery] Guid buyer)
        {
            var result = await _cartService.CountItemsInCartAsync(buyer);
            return HandleResponse(result);
        }

        /// <summary>
        /// Thêm item vào giỏ hàng
        /// </summary>
        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem([FromQuery] Guid buyer, [FromBody] RequestItemToCartModel item)
        {
            if (item == null)
                return BadRequest("Invalid item data");

            var result = await _cartService.AddItemToCartAsync(buyer, item);
            return HandleResponse(result);
        }

        /// <summary>
        /// Cập nhật quantity của một item trong giỏ hàng
        /// </summary>
        [HttpPut("update-item")]
        public async Task<IActionResult> UpdateItemQuantity(
                    [FromQuery] Guid buyer,
                    [FromQuery] Guid item,
                    [FromBody] UpdateQuantityRequest request)
        {
            if (request == null || request.Quantity <= 0)
                return BadRequest(request);

            // Gọi update service (đã validate bên trong)
            var result = await _cartService.UpdateItemAsync(buyer, item, request);
           
            return HandleResponse(result);
        }

        /// <summary>
        /// Xóa item khỏi giỏ hàng
        /// </summary>
        [HttpDelete("/delete-item")]
        public async Task<IActionResult> RemoveItem([FromQuery]  Guid buyer, [FromQuery] Guid item)
        {
            var result = await _cartService.RemoveItemFromCartAsync(buyer, item);
            return HandleResponse(result);
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        [HttpDelete("delete-cart")]
        public async Task<IActionResult> ClearCart([FromQuery] Guid buyer)
        {
            var result = await _cartService.ClearCartAsync(buyer);
            return HandleResponse(result);
        }


        /// <summary>
        /// Lấy các items trong giỏ hàng theo store
        /// </summary>
        [HttpGet("{userId:guid}/stores/{storeId:guid}/items")]
        public async Task<IActionResult> GetCartItemsByStore(Guid userId, Guid storeId)
        {
            var result = await _cartService.GetCartItemsByStoreAsync(userId, storeId);
            return HandleResponse(result);
        }

        private IActionResult HandleResponse<T>(OrderResponseModel<T> response)
        {
            return response.Message switch
            {
                OperationResult.Success => Ok(new { statusCode = 200, message = "Operation successful", data = response.Data }),
                OperationResult.Failed => BadRequest(new { statusCode = 400, message = response.ErrorMessage ?? "Operation failed" }),
                OperationResult.Forbidden => StatusCode(403, new { statusCode = 403, message = response.ErrorMessage ?? "Forbidden - You don't have permission" }),
                OperationResult.NotFound => NotFound(new { statusCode = 404, message = response.ErrorMessage ?? "Not found" }),
                OperationResult.Conflict => Conflict(new { statusCode = 409, message = response.ErrorMessage ?? "Conflict occurred" }),
                OperationResult.Error => StatusCode(500, new { statusCode = 500, message = response.ErrorMessage ?? "Unexpected error!" }),
                _ => StatusCode(500, new { statusCode = 500, message = "Unexpected error!" })
            };
        }
    }

  

    
}
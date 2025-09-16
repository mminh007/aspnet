using Microsoft.AspNetCore.Mvc;
using Oder.BLL.Services.Interfaces;
using Order.BLL.Services;
using Order.Common.Enums;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using static Order.Common.Models.DTOs;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var result = await _cartService.GetCartAsync(userId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Thêm item vào giỏ hàng
        /// </summary>
        [HttpPost("{userId:guid}/items")]
        public async Task<IActionResult> AddItem(Guid userId, [FromBody] RequestItemToCartModel item)
        {
            if (item == null)
                return BadRequest("Invalid item data");

            var result = await _cartService.AddItemToCartAsync(userId, item);
            return HandleResponse(result);
        }

        /// <summary>
        /// Xóa item khỏi giỏ hàng
        /// </summary>
        [HttpDelete("{userId:guid}/items/{productId:guid}")]
        public async Task<IActionResult> RemoveItem(Guid userId, Guid productId)
        {
            var result = await _cartService.RemoveItemFromCartAsync(userId, productId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            var result = await _cartService.ClearCartAsync(userId);
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

        /// <summary>
        /// Cập nhật quantity của một item trong giỏ hàng
        /// </summary>
        [HttpPut("{userId:guid}/items/{productId:guid}/quantity")]
        public async Task<IActionResult> UpdateItemQuantity(Guid userId, Guid productId, [FromBody] UpdateQuantityRequest request)
        {
            if (request == null || request.Quantity <= 0)
                return BadRequest(request);

            // Lấy giỏ hàng hiện tại
            var cartResponse = await _cartService.GetCartAsync(userId);
            if (!cartResponse.Success || cartResponse.Data == null)
                return HandleResponse(cartResponse);

            // Tìm item cần update
            var item = cartResponse.Data.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return NotFound("Item not found in cart");

            // Xóa item cũ và thêm lại với quantity mới
            await _cartService.RemoveItemFromCartAsync(userId, productId);

            var newItem = new RequestItemToCartModel
            {
                ProductId = item.ProductId,
                StoreId = item.StoreId,
                Quantity = request.Quantity,
            };

            item.Quantity = request.Quantity;
            var addResult = await _cartService.AddItemToCartAsync(userId, newItem);

            return HandleResponse(addResult);
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
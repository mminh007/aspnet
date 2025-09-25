using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oder.BLL.Services.Interfaces;
using Order.Common.Enums;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using static Order.Common.Models.DTOs;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }       

        // ✅ Lấy đơn hàng theo Id
        [HttpGet("get-order")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> GetOrderById([FromQuery] Guid order)
        {
            var result = await _orderService.GetOrderByIdAsync(order);
            return HandleResponse(result);
        }

        // ✅ Lấy danh sách đơn theo User
        [HttpGet("user/get")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> GetOrdersByUser([FromQuery] Guid buyer)
        {
            var result = await _orderService.GetOrdersByUserAsync(buyer);
            return HandleResponse(result);
        }

        // ✅ Lấy danh sách đơn theo Store
        [HttpGet("seller")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> GetOrdersByStore([FromQuery] Guid store)
        {
            var result = await _orderService.GetOrdersByStoreAsync(store);
            return HandleResponse(result);
        }

        // ✅ Cập nhật đơn hàng (Payment Service sẽ gọi API này để cập nhật trạng thái thanh toán)
        [HttpPut("update-order")]
        [Authorize(Roles = "system")]
        public async Task<IActionResult> UpdateOrder([FromQuery] Guid order, [FromBody] UpdateOrderRequest dto)
        {
            if (dto == null || dto.OrderId != order)
                return BadRequest("Invalid order data");

            var result = await _orderService.UpdateOrderAsync(dto);
            return HandleResponse(result);
        }

        // ✅ Xóa đơn hàng
        [HttpDelete("delete-order")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> DeleteOrder([FromQuery] Guid order)
        {
            var result = await _orderService.DeleteOrderAsync(order);
            return HandleResponse(result);
        }

        // ✅ Checkout từ giỏ
        [HttpPost("checkout")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> Checkout([FromQuery] Guid user, [FromBody] IEnumerable<Guid> productIds)
        {
            if (productIds == null || !productIds.Any())
                return BadRequest("No products selected for checkout");

            var result = await _orderService.CheckoutAsync(user, productIds);
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

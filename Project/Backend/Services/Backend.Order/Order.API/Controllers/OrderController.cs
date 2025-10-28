using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.BLL.Services.Interfaces;
using Order.Common.Enums;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


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
        public async Task<IActionResult> GetOrdersByUser()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            var result = await _orderService.GetOrdersByUserAsync(userId);
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
        [HttpPost("create-order")]
        [Authorize(Roles = "buyer")]
        public async Task<IActionResult> CreateOrder([FromBody] RequestOrderModel request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

            if (request.ProductIds == null || !request.ProductIds.Any())
                return BadRequest("No products selected for checkout");

            var result = await _orderService.CheckoutAsync(userId, request);
            return HandleResponse(result);
        }

        [HttpPut("update-status/{orderId}")]
        [Authorize(Roles= "system")]
        public async Task<IActionResult> UpdateStatusOrder(Guid orderId, [FromQuery] string status, [FromBody] decimal totalAmount)
        {
            var response = await _orderService.UpdateStatusAsync(orderId, status, totalAmount);

            return HandleResponse(response);
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

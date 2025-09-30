using Frontend.Models.Orders.Requests;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using static Frontend.Models.Orders.DTOs;

namespace Frontend.Controllers
{
    [Route("Order")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> CreateCart(Guid id)
        {
            var(message, status, data) = await _orderService.GetCartByUserId(id, "");
            if(status != 200)
            {
                ViewBag.Error = message;
            }

            //_logger.LogInformation("CartItem in store: {@Data}", JsonSerializer.Serialize(data, new JsonSerializerOptions
            //{
            //    WriteIndented = true
            //}));

            return View(data);
        }

        [HttpGet("get-cart-in-store")]
        public async Task<IActionResult> GetCartItemInStore(Guid buyer, Guid storeId)
        {
            var (cartMsg, cartStatus, cartItems) = await _orderService.GetCartInStore(buyer, storeId);

            return Ok( new
            {
                message = cartMsg, 
                data = cartItems 
            });  
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity(
            [FromQuery] Guid buyer, 
            [FromQuery] Guid item,
            [FromQuery] Guid store,
            [FromBody] UpdateQuantityModel request)
        {
            var (msg, status, data) = await _orderService.UpdateItemsInCart(buyer, item, request);

            //_logger.LogInformation("CartDTO result: {CartJson}",
            //JsonSerializer.Serialize(data, new JsonSerializerOptions
            //{
            //    WriteIndented = true 
            //}));

            // Bug
            // ✅ Lọc ra các CartItem theo storeId
            var itemsInStore = data.Items
                                   .Where(i => i.StoreId == store)
                                   .ToList();

            // ✅ Tính tổng số lượng trong store
            var totalItemsInStore = itemsInStore.Sum(i => i.Quantity);

            return Ok( new
            {
                message = msg,
                data = data,
                countItemsInStore = totalItemsInStore
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Guid userId, [FromForm] List<Guid> selectedProducts)
        {
            if (selectedProducts == null || !selectedProducts.Any())
            {
                TempData["Error"] = "Bạn chưa chọn sản phẩm nào để tạo đơn hàng.";
                return RedirectToAction("CreateCart", new { id = userId });
            }

            var (msg, status, data) = await _orderService.Checkout(userId, selectedProducts);

            if (status != 200 || data == null)
            {
                ViewBag.Error = msg;
                return View("CreateOrder", new List<OrderDTO>());
            }

            return View("CreateOrder", data);
        }

        [HttpDelete("delete-item")]
        public async Task<IActionResult> DeleteCartItem(Guid buyer, [FromQuery] Guid item)
        {
            var (msg, status, data) = await _orderService.DeleteItemInCart(buyer, item);

            if (status == 200)
            {
                // ✅ Trả về giỏ hàng mới nhất
                return Ok(new
                {
                    statusCode = 200,
                    message = "Item removed successfully",
                    data = data // CartDTO
                });
            }

            // ❌ Lỗi
            return StatusCode(status, new
            {
                statusCode = status,
                message = msg
            });
        }

        [HttpGet("list-order")]
        public async Task<IActionResult> GetOrderList(Guid buyer)
        {
            var (msg, status, data) = await _orderService.GetOrdersByUser(buyer);

            if (status != 200)
            {
                ViewBag.Error = msg;
            }

            return View(data);

        }

    }
}

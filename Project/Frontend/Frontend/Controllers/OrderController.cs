using Frontend.Models.Orders.Requests;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static Frontend.Models.Orders.DTOs;

namespace Frontend.Controllers
{
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

        [HttpPut]
        public async Task<IActionResult> UpdateQuantity(
            [FromQuery] Guid userId, 
            [FromQuery] Guid cartItemId, 
            [FromBody] UpdateQuantityModel request)
        {
            var (msg, status, data) = await _orderService.UpdateItemsInCart(userId, cartItemId, request);

            _logger.LogInformation("CartDTO result: {CartJson}",
            JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true // để format đẹp dễ đọc
            }));
            if (status != 200)
            {
                ViewBag.Error = msg;
            }
            return Ok( new
            {
                message = msg,
                data = data

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

    }
}

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
        private readonly IConfiguration _config;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger, IConfiguration config)
        {
            _orderService = orderService;
            _logger = logger;
            _config = config;
        }

        [HttpGet("get-cart")]
        public async Task<IActionResult> GetCart(Guid id)
        {
            var (message, status, data) = await _orderService.GetCartByUserId(id, "check");
            if (status != 200 || data == null)
            {
                return StatusCode(status, new { message });
            }

            return Ok(new
            {
                message,
                data // CartDTO
            });
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

        [HttpPut("update-qty")]
        public async Task<IActionResult> UpdateQty(
            [FromQuery] Guid buyer,
            [FromQuery] Guid id,
            [FromQuery] Guid store,
            [FromBody] RequestUpdate request)
        {
            var updateProd = new UpdateQuantityModel
            {
                Quantity = request.Quantity,
                Productid = request.Productid
            };

            var (msg, status, data) = await _orderService.UpdateItemsInCart(buyer, store, id, updateProd);

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

            return Ok(new
            {
                message = msg,
                data = data,
                countItemsInStore = totalItemsInStore
            });
        }

        //[HttpPut("update-quantity")]
        //public async Task<IActionResult> UpdateQuantity(
        //    [FromQuery] Guid buyer,
        //    [FromQuery] Guid store,
        //    [FromBody] RequestUpdate request)
        //{
        //    var updateProd = new UpdateQuantityModel
        //    {
        //        Quantity = request.Quantity,
        //        Productid = request.Productid
        //    };

        //    var (msg, status, data) = await _orderService.UpdateItemsInCart(buyer, store, request.CartItemId, updateProd);

        //    //_logger.LogInformation("CartDTO result: {CartJson}",
        //    //JsonSerializer.Serialize(data, new JsonSerializerOptions
        //    //{
        //    //    WriteIndented = true 
        //    //}));

        //    // Bug
        //    // ✅ Lọc ra các CartItem theo storeId
        //    var itemsInStore = data.Items
        //                           .Where(i => i.StoreId == store)
        //                           .ToList();

        //    // ✅ Tính tổng số lượng trong store
        //    var totalItemsInStore = itemsInStore.Sum(i => i.Quantity);

        //    return Ok( new
        //    {
        //        message = msg,
        //        data = data,
        //        countItemsInStore = totalItemsInStore
        //    });
        //}

        


        [HttpPost]
        public async Task<IActionResult> CreateOrder(Guid userId, [FromForm] List<Guid> selectedProducts, [FromForm] RequestOrderModel shipping)
        {
            ViewBag.StripePublishableKey = _config["STRIPE:PUBLISHABLEKEY"];

            if (shipping.Note == null)
            {
                shipping.Note = string.Empty;
            }
            if (selectedProducts == null || !selectedProducts.Any())
            {
                TempData["Error"] = "Bạn chưa chọn sản phẩm nào để tạo đơn hàng.";
                return RedirectToAction("CreateCart", new { id = userId });
            }

            if (string.IsNullOrWhiteSpace(shipping.FullName) ||
                string.IsNullOrWhiteSpace(shipping.PhoneNumber) ||
                string.IsNullOrWhiteSpace(shipping.Address))
            {
                TempData["Error"] = "Thông tin giao hàng không hợp lệ.";
                return RedirectToAction("CreateCart", new { id = userId });
            }

            var (msg, status, data) = await _orderService.CreateOrder(userId, selectedProducts, shipping);

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
        public async Task<IActionResult> GetOrderList()
        {
            var buyerIdStr = HttpContext.Session.GetString("UserId");

            var buyer = Guid.Parse(buyerIdStr);

            var (msg, status, data) = await _orderService.GetOrdersByUser(buyer);

            if (status != 200)
            {
                ViewBag.Error = msg;
            }

            return View(data);

        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder([FromQuery] Guid order_id)
        {
            _logger.LogInformation("CancelOrder called with order_id: {OrderId}", order_id);

            if(order_id == Guid.Empty)
            {
                ViewBag.Error = "Order ID is required to cancel an order.";
                return RedirectToAction("GetOrderList");
            }

            var (msg, status) = await _orderService.CancelOrder(order_id);
            if (status == 200)
            {
                return RedirectToAction("GetOrderList");
            }
            ViewBag.Error = msg;
            return RedirectToAction("GetOrderList");
        }
        

    }
}

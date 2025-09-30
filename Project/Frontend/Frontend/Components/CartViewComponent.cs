using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Components
{
    public class CartViewComponent : ViewComponent
    {
        private readonly IOrderService _orderService;

        public CartViewComponent(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            int countItems = 0;

            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsedId))
            {
                try
                {
                    var (msg, status, data) = await _orderService.CountingItemsInCart(parsedId);

                    if (status == 200 && data != null)
                    {
                        countItems = data.CountItems;
                    }
                    else
                    {
                        // nếu Unauthorized hoặc lỗi khác thì fallback 0
                        countItems = 0;
                    }
                }
                catch (Exception ex)
                {
                    // log lỗi nhưng không crash
                    Console.WriteLine($"CartViewComponent error: {ex.Message}");
                    countItems = 0;
                }
            }

            return View(countItems);
        }

    }
}

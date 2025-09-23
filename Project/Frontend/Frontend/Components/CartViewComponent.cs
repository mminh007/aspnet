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

            if (Guid.TryParse(userId, out var parsedId))
            {
                var (msg, status, data) = await _orderService.CountingItemsInCart(parsedId);
                countItems = data?.CountItems ?? 0;
            }

            return View(countItems);
        }
    }
}

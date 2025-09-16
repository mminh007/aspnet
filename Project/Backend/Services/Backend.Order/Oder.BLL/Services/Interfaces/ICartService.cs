using Order.Common.Models;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using static Order.Common.Models.DTOs;

namespace Order.BLL.Services
{
    public interface ICartService
    {
        Task<OrderResponseModel<CartDTO?>> GetCartAsync(Guid userId);
        Task<OrderResponseModel<Guid>> AddItemToCartAsync(Guid userId, RequestItemToCartModel itemDto);
        Task<OrderResponseModel<string>> RemoveItemFromCartAsync(Guid userId, Guid productId);
        Task<OrderResponseModel<string>> ClearCartAsync(Guid userId);
        Task<OrderResponseModel<List<CartItemDTO>>> CheckoutAsync(Guid userId, IEnumerable<Guid> productIds);
        Task<OrderResponseModel<List<CartItemDTO>>> GetCartItemsByStoreAsync(Guid userId, Guid storeId);

    }
}

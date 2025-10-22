using Order.Common.Models;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using static Order.Common.Models.DTOs;

namespace Order.BLL.Services
{
    public interface ICartService
    {
        Task<OrderResponseModel<int>> CreateCart(Guid userId);
        Task<OrderResponseModel<CartDTO?>> GetCartAsync(Guid userId, string act="check", bool noTracking=false);
        Task<OrderResponseModel<CartDTO>> AddItemToCartAsync(Guid userId, RequestItemToCartModel itemDto);
        Task<OrderResponseModel<CartDTO>> RemoveItemFromCartAsync(Guid userId, Guid cartItemId);
        Task<OrderResponseModel<string>> ClearCartAsync(Guid userId);
        Task<OrderResponseModel<List<CartItemDTO>>> CheckoutAsync(Guid userId, IEnumerable<Guid> productIds);
        Task<OrderResponseModel<List<CartItemDTO>>> GetCartItemsByStoreAsync(Guid userId, Guid storeId);
        Task<OrderResponseModel<CountItemsDTO>> CountItemsInCartAsync(Guid userId);
        Task<OrderResponseModel<CartDTO>> UpdateItemAsync(Guid buyerId, Guid cartItemId, UpdateQuantityRequest request);


    }
}

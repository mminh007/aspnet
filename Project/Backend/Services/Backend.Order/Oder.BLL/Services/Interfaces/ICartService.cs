using Order.Common.Models;
using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using static Order.Common.Models.DTOs;

namespace Order.BLL.Services
{
    public interface ICartService
    {
        Task<OrderResponseModel<CartDTO?>> GetCartAsync(Guid userId, string act="check");
        Task<OrderResponseModel<int>> AddItemToCartAsync(Guid userId, RequestItemToCartModel itemDto);
        Task<OrderResponseModel<string>> RemoveItemFromCartAsync(Guid userId, Guid productId);
        Task<OrderResponseModel<string>> ClearCartAsync(Guid userId);
        Task<OrderResponseModel<List<CartItemDTO>>> CheckoutAsync(Guid userId, IEnumerable<Guid> productIds);
        Task<OrderResponseModel<List<CartItemDTO>>> GetCartItemsByStoreAsync(Guid userId, Guid storeId);
        Task<OrderResponseModel<CountItemsDTO>> CountItemsInCartAsync(Guid userId);
        Task<OrderResponseModel<CartDTO>> UpdateItemAsync(Guid buyerId, Guid cartItemId, UpdateQuantityRequest request);


    }
}

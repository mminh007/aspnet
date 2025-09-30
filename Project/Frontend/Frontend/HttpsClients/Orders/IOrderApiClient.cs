using Frontend.Models.Orders;
using Frontend.Models.Orders.Requests;

namespace Frontend.HttpsClients.Orders
{
    public interface IOrderApiClient
    {
        Task<(bool Success, string? Message, int statusCode, DTOs.CartDTO data)> GetCart(Guid userId);

        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.CartItemDTO> data)> GetCartItemInStore(Guid storeId);
        Task<(bool Success, string? Message, int statusCode, DTOs.CountItemsDTO data)> GetCountItemsToCart(Guid userId);
        Task<(bool Success, string? Message, int statusCode, DTOs.CartDTO data)> AddItemsToCart(Guid userId, RequestItemsToCartModel request);
        Task<(bool Success, string? Message, int statusCode, DTOs.CartDTO data)> UpdateItemQuantity(Guid userId, Guid cartItemId, UpdateQuantityModel request);
        Task<(bool Success, string? Message, int statusCode, DTOs.CartDTO data)> RemoveItem(Guid userId, Guid cartItemId);
        Task<(bool Success, string? Message, int statusCode)> ClearCart(Guid userId);


        // Order APIs (role = buyer)
        Task<(bool Success, string? Message, int statusCode, DTOs.OrderDTO data)> GetOrderById(Guid orderId);
        Task<(bool Success, string? Message, int statusCode, List<DTOs.OrderDTO> data)> GetOrdersByUser(Guid userId);
        Task<(bool Success, string? Message, int statusCode)> DeleteOrder(Guid orderId);
        Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.OrderDTO> data)> Checkout(Guid userId, IEnumerable<Guid> productIds);

    }
}

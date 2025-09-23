using Frontend.Models.Orders;
using Frontend.Models.Orders.Requests;

namespace Frontend.HttpsClients.Orders
{
    public interface IOrderApiClient
    {
        Task<(bool Success, string? Message, int statusCode, DTOs.CartDTO data)> GetCart(Guid userId);
        Task<(bool Success, string? Message, int statusCode, DTOs.CountItemsDTO data)> GetCountItemsToCart(Guid userId);
        Task<(bool Success, string? Message, int statusCode, int TotalItems)> AddItemsToCart(Guid userId, RequestItemsToCartModel request);
        Task<(bool Success, string? Message, int statusCode, DTOs.CartDTO data)> UpdateItemQuantity(Guid userId, Guid cartItemId, UpdateQuantityModel request);
        Task<(bool Success, string? Message, int statusCode)> RemoveItem(Guid userId, Guid productId);
        Task<(bool Success, string? Message, int statusCode)> ClearCart(Guid userId);

    }
}

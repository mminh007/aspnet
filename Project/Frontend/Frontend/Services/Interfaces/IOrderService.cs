using Frontend.Models.Orders;
using Frontend.Models.Orders.Requests;

namespace Frontend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<(string Message, int StatusCode, DTOs.CountItemsDTO? data)> CountingItemsInCart(Guid userId);

        Task<(string Message, int StatusCode, int CountItems)> AddProductToCart(Guid userId,  RequestItemsToCartModel dto);

        Task<(string Message, int StatusCode, DTOs.CartDTO? data)> GetCartByUserId(Guid userId, string status);

        Task<(string Message, int StatusCode, DTOs.CartDTO data)> UpdateItemsInCart(Guid userId, Guid itemId, UpdateQuantityModel request);
    }
}

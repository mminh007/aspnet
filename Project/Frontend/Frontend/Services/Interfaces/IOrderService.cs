using Frontend.Models.Orders;

namespace Frontend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<(string Message, int StatusCode, DTOs.CountItemsDTO? dto)> CountingItemsInCart(Guid userId);

        Task<(string Message, int CountItems)> AddProductToCache(Guid userId, Guid cartId, Guid productId);
    }
}

using Frontend.Models.Orders;

namespace Frontend.HttpsClients.Orders
{
    public interface IOrderApiClient
    {
        Task<(bool Success, string? Message, int statusCode, DTOs.CountItemsDTO dto)> GetCountItemsToCart(Guid userId);

        Task<(bool Success, string? Message, int statusCode, Guid cartId)> AddItemsToCart(DTOs.AddItemToCartRequest request);

    }
}

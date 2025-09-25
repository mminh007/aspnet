using Frontend.Models.Orders;
using Frontend.Models.Orders.Requests;

namespace Frontend.Services.Interfaces
{
    public interface IOrderService
    {
        Task<(string Message, int StatusCode, DTOs.CountItemsDTO? data)> CountingItemsInCart(Guid userId);

        Task<(string Message, int StatusCode, int CountItems, IEnumerable<DTOs.CartItemDTO> itemList)> AddProductToCart(Guid userId,  RequestItemsToCartModel dto);

        Task<(string Message, int StatusCode, DTOs.CartDTO? data)> GetCartByUserId(Guid userId, string status);

        Task<(string Message, int StatusCode, DTOs.CartDTO data)> UpdateItemsInCart(Guid userId, Guid itemId, UpdateQuantityModel request);

        Task<(string Message, int StatusCode, IEnumerable<DTOs.CartItemDTO> itemList)> GetCartInStore(Guid userId, Guid storeId);


        // Order function

        Task<(string Message, int StatusCode, DTOs.OrderDTO Data)>
            GetOrderById(Guid orderId);

        Task<(string Message, int StatusCode, List<DTOs.OrderDTO> Data)>
            GetOrdersByUser(Guid userId);

        Task<(string Message, int StatusCode)>
            DeleteOrder(Guid orderId);

        Task<(string Message, int StatusCode, DTOs.OrderDTO Data)>
            Checkout(Guid userId, IEnumerable<Guid> productIds);

    }
}

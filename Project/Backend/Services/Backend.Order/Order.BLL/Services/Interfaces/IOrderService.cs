using Order.Common.Models.Requests;
using Order.Common.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Order.Common.Models.DTOs;

namespace Order.BLL.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseModel<OrderDTO>> CreateOrderAsync(OrderDTO dto);
        Task<OrderResponseModel<OrderDTO?>> GetOrderByIdAsync(Guid orderId);
        Task<OrderResponseModel<IEnumerable<OrderDTO>>> GetOrdersByUserAsync(Guid userId);
        Task<OrderResponseModel<IEnumerable<OrderDTO>>> GetOrdersByStoreAsync(Guid storeId);
        Task<OrderResponseModel<OrderDTO>> UpdateOrderAsync(UpdateOrderRequest dto); // update payment status, payment method, order status, dont update products
        Task<OrderResponseModel<string>> DeleteOrderAsync(Guid orderId);

        Task<OrderResponseModel<string>> UpdateStatusAsync(Guid orderId, string status);



        // Checkout từ cart
        Task<OrderResponseModel<IEnumerable<OrderDTO>>> CheckoutAsync(Guid userId, IEnumerable<Guid> productIds);
    }
}

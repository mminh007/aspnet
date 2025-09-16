using Order.DAL.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.DAL.Repository.Interfaces
{
    public interface IOrderRepository
    {
        Task<OrderModel?> GetOrderByIdAsync(Guid orderId);
        Task<List<OrderModel>> GetOrdersByUserAsync(Guid userId);
        Task<List<OrderModel>> GetOrdersByStoreAsync(Guid storeId);

        Task CreateOrderAsync(OrderModel order);
        Task UpdateOrderAsync(OrderModel order);
        Task DeleteOrderAsync(Guid orderId);

        // === Business use case ===
        Task<OrderModel> CreateOrderFromCartAsync(CartModel cart, Guid storeId);
    }
}

using Microsoft.EntityFrameworkCore;
using Order.DAL.Databases;
using Order.DAL.Models.Entities;
using Order.DAL.Repository.Interfaces;

namespace Order.DAL.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        public Task<OrderModel?> GetOrderByIdAsync(Guid orderId) =>
            _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

        public Task<List<OrderModel>> GetOrdersByUserAsync(Guid userId) =>
            _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .ToListAsync();

        public Task<List<OrderModel>> GetOrdersByStoreAsync(Guid storeId) =>
            _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.StoreId == storeId)
                .ToListAsync();

        public async Task CreateOrderAsync(OrderModel order)
        {
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.Orders.AddAsync(order);
        }

        public Task UpdateOrderAsync(OrderModel order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task DeleteOrderAsync(Guid orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
        }

        // === Business use case ===
        public async Task<OrderModel> CreateOrderFromCartAsync(CartModel cart, Guid storeId)
        {
            var order = new OrderModel
            {
                OrderId = Guid.NewGuid(),
                UserId = cart.UserId,
                StoreId = storeId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = cart.Items.Select(ci => new OrderItemModel
                {
                    OrderItemId = Guid.NewGuid(),
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Price
                }).ToList()
            };

            await _context.Orders.AddAsync(order);
            return order;
        }
    }
}

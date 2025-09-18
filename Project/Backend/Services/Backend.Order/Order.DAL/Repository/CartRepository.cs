using Microsoft.EntityFrameworkCore;
using Order.DAL.Databases;
using Order.DAL.Models.Entities;
using Order.DAL.Repository.Interfaces;
using static Order.Common.Models.DTOs;

namespace Order.DAL.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly OrderDbContext _context;

        public CartRepository(OrderDbContext context)
        {
            _context = context;
        }

        public Task<CartModel?> GetCartByUserIdAsync(Guid userId) =>
            _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

        public Task<CartModel?> GetCartByIdAsync(Guid cartId) =>
            _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CartId == cartId);

        // 👉 Thêm filter theo store
        public async Task<List<CartItemModel>> GetCartItemsByStoreAsync(Guid userId, Guid storeId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.Items.Where(i => i.StoreId == storeId).ToList() ?? new List<CartItemModel>();
        }

        public async Task CreateCartAsync(CartModel cart)
        {
            cart.CreatedAt = DateTime.UtcNow;
            await _context.Carts.AddAsync(cart);
        }

        public Task UpdateCartAsync(CartModel cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(cart);
            return Task.CompletedTask;
        }

        public async Task DeleteCartAsync(Guid cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }
        }

        public Task AddCartItemAsync(CartItemModel item) =>
            _context.CartItems.AddAsync(item).AsTask();

        public Task UpdateCartItemAsync(CartItemModel item)
        {
            _context.CartItems.Update(item);
            return Task.CompletedTask;
        }

        public async Task RemoveCartItemAsync(Guid cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
            }
        }

        // === Business use case ===
        public async Task ClearCartAsync(Guid cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CartId == cartId);

            if (cart != null && cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
            }
        }

       
    }
}

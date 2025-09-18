using Order.DAL.Models.Entities;
using static Order.Common.Models.DTOs;

namespace Order.DAL.Repositories
{
    public interface ICartRepository
    {
        Task<CartModel?> GetCartByUserIdAsync(Guid userId);
        Task<CartModel?> GetCartByIdAsync(Guid cartId);
        Task<List<CartItemModel>> GetCartItemsByStoreAsync(Guid userId, Guid storeId);
        Task CreateCartAsync(CartModel cart);
        Task UpdateCartAsync(CartModel cart);
        Task DeleteCartAsync(Guid cartId);

        Task AddCartItemAsync(CartItemModel item);
        Task UpdateCartItemAsync(CartItemModel item);
        Task RemoveCartItemAsync(Guid cartItemId);
        Task ClearCartAsync(Guid cartId);
    }
}

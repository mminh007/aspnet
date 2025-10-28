using Order.DAL.Databases;
using Order.DAL.Repository.Interfaces;
using Order.DAL.UnitOfWork.Interfaces;

namespace Order.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _context;

        public ICartRepository Carts { get; }
        public IOrderRepository Orders { get; }

        public IShippingRepository Shippings { get; }

        public UnitOfWork(OrderDbContext context)
        {
            _context = context;
            Carts = new CartRepository(context);
            Orders = new OrderRepository(context);
            Shippings = new ShippingRepository(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Order.DAL.Databases;
using Order.DAL.Models.Entities;
using Order.DAL.Repository.Interfaces;

namespace Order.DAL.Repositories
{
    public class ShippingRepository : IShippingRepository
    {
        private readonly OrderDbContext _db;

        public ShippingRepository(OrderDbContext db)
        {
            _db = db;
        }

        public async Task<ShippingModel> AddAsync(ShippingModel entity, CancellationToken ct = default)
        {
            if (entity.ShippingId == Guid.Empty)
                entity.ShippingId = Guid.NewGuid();

            await _db.Shippings.AddAsync(entity, ct);
            return entity;
        }

        public Task<ShippingModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _db.Shippings.AsNoTracking().FirstOrDefaultAsync(x => x.ShippingId == id, ct);
        }

        public async Task UpdateAsync(ShippingModel entity, CancellationToken ct = default)
        {
            _db.Shippings.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var existing = await _db.Shippings.FirstOrDefaultAsync(x => x.ShippingId == id, ct);
            if (existing != null)
                _db.Shippings.Remove(existing);
        }
    }
}

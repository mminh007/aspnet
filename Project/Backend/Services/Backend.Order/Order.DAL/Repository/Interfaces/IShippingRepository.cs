using Order.DAL.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.DAL.Repository.Interfaces
{
    public interface IShippingRepository
    {
        Task<ShippingModel> AddAsync(ShippingModel entity, CancellationToken ct = default);
        Task<ShippingModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(ShippingModel entity, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}

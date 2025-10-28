using Order.DAL.Repositories;
using Order.DAL.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.DAL.UnitOfWork.Interfaces
{
    public interface IUnitOfWork
    {
        ICartRepository Carts { get; }
        IOrderRepository Orders { get; }

        IShippingRepository Shippings { get; }

        Task<int> SaveChangesAsync();
    }
}

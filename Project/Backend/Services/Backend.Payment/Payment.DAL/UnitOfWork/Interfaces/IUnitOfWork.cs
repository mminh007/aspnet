using System;
using System.Threading.Tasks;
using Payment.DAL.Repository.Interfaces;

namespace Payment.DAL.UnitOfWork.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IPaymentRepository Payments { get; }
        Task<int> SaveChangesAsync();
    }
}

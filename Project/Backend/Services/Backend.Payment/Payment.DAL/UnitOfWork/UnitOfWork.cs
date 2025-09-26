using System;
using System.Threading.Tasks;
using Payment.DAL.Repository;
using Payment.DAL.Repository.Interfaces;
using Payment.DAL.UnitOfWork.Interfaces;

namespace Payment.DAL.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentDbContext _context;
        public IPaymentRepository Payments { get; }

        public UnitOfWork(PaymentDbContext context)
        {
            _context = context;
            Payments = new PaymentRepository(_context);
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

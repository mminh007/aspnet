using Microsoft.EntityFrameworkCore;
using Payment.Common.Enums;
using Payment.DAL.Models.Entities;
using Payment.DAL.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Payment.DAL.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<PaymentModel>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            return await _context.Payments
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PaymentModel?> GetByIdAsync(Guid id)
        {
            return await _context.Payments.FindAsync(id);
        }

        public async Task<PaymentModel?> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<IEnumerable<PaymentModel>> GetByBuyerAsync(Guid buyerId)
        {
            return await _context.Payments
                .Where(p => p.BuyerId == buyerId)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentModel>> GetByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentModel>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.Payments
                .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<PaymentModel>> FindAsync(Expression<Func<PaymentModel, bool>> predicate)
        {
            return await _context.Payments
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Payments.CountAsync();
        }

        public async Task<int> CountByStatusAsync(PaymentStatus status)
        {
            return await _context.Payments.CountAsync(p => p.Status == status);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Payments.AnyAsync(p => p.PaymentId == id);
        }

        public async Task AddAsync(PaymentModel payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            await _context.Payments.AddAsync(payment);
        }

        public void Update(PaymentModel payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            _context.Entry(payment).State = EntityState.Modified;
        }

        public void Delete(PaymentModel payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));
            _context.Payments.Remove(payment);
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            var payment = await GetByIdAsync(id);
            if (payment != null)
            {
                Delete(payment);
            }
        }

        public async Task AddRangeAsync(IEnumerable<PaymentModel> payments)
        {
            if (payments == null) throw new ArgumentNullException(nameof(payments));
            await _context.Payments.AddRangeAsync(payments);
        }

        public void UpdateRange(IEnumerable<PaymentModel> payments)
        {
            if (payments == null) throw new ArgumentNullException(nameof(payments));
            _context.Payments.UpdateRange(payments);
        }

        public void DeleteRange(IEnumerable<PaymentModel> payments)
        {
            if (payments == null) throw new ArgumentNullException(nameof(payments));
            _context.Payments.RemoveRange(payments);
        }
    }
}

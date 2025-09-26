using Payment.Common.Enums;
using Payment.DAL.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Payment.DAL.Repository.Interfaces
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<PaymentModel>> GetAllAsync(int page = 1, int pageSize = 50);
        Task<PaymentModel?> GetByIdAsync(Guid id);
        Task<PaymentModel?> GetByOrderIdAsync(Guid orderId);
        Task<IEnumerable<PaymentModel>> GetByBuyerAsync(Guid buyerId);

        Task<IEnumerable<PaymentModel>> GetByStatusAsync(PaymentStatus status);
        Task<IEnumerable<PaymentModel>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<IEnumerable<PaymentModel>> FindAsync(Expression<Func<PaymentModel, bool>> predicate);

        // Count methods
        Task<int> CountAsync();
        Task<int> CountByStatusAsync(PaymentStatus status);
        Task<bool> ExistsAsync(Guid id);

        // CUD operations
        Task AddAsync(PaymentModel payment);
        void Update(PaymentModel payment);
        void Delete(PaymentModel payment);
        Task DeleteByIdAsync(Guid id);

        // Bulk operations
        Task AddRangeAsync(IEnumerable<PaymentModel> payments);
        void UpdateRange(IEnumerable<PaymentModel> payments);
        void DeleteRange(IEnumerable<PaymentModel> payments);
    }
}

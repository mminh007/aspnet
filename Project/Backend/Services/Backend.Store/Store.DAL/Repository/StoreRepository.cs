using Microsoft.EntityFrameworkCore;
using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using Store.DAL.Databases;
using Store.DAL.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Store.DAL.Repository
{
    public class StoreRepository : IStoreRepository
    {
        private readonly StoreDbContext _db;

        public StoreRepository(StoreDbContext db)
        {

            _db = db;
        }

        public async Task<IEnumerable<StoreDTO>> SearchStoreByKeywordPageAsync(string keyword, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
                
            var query = _db.Stores
                //.Where(s => RemoveDiacritics(s.StoreName ?? "").ToLower().Contains(RemoveDiacritics(keyword).ToLower()))
                .Where(s => EF.Functions.Collate(s.StoreName, "SQL_Latin1_General_CP1_CI_AI")
                            .Contains(EF.Functions.Collate(keyword, "SQL_Latin1_General_CP1_CI_AI")))
                .OrderByDescending(s => s.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(s => new StoreDTO
                {
                    StoreId = s.StoreId,
                    StoreCategory = s.StoreCategory,
                    StoreName = s.StoreName,
                    Address = s.Address,
                    Description = s.Description,
                    StoreImage = s.StoreImage,
                    IsActive = s.IsActive
                });

            return await query.ToListAsync();
        }

        public async Task<int> CountStoreByKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return 0;

            return await _db.Stores
                .Where(s => (s.StoreName ?? "").Contains(keyword))
                .CountAsync();
        }

        public async Task<IEnumerable<StoreDTO>> SearchStoreByTagPagedAsync(string tagSlug, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var query = _db.Stores
                .Where(s => string.IsNullOrEmpty(tagSlug) || s.StoreCategorySlug.Contains(tagSlug))
                .OrderByDescending(s => s.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(s => new StoreDTO
                {
                    StoreId = s.StoreId,
                    StoreCategory  = s.StoreCategory,
                    StoreName = s.StoreName,
                    Address = s.Address,
                    Description = s.Description,
                    StoreImage = s.StoreImage,
                    IsActive = s.IsActive
                });

            return await query.ToListAsync();
        }

        public async Task<int> CountStoreByTagAsync(string tagSlug)
        {
            return await _db.Stores
                .Where(s => string.IsNullOrEmpty(tagSlug) || s.StoreCategorySlug.Contains(tagSlug))
                .CountAsync();
        }


        public async Task<Guid> CreateStoreAsync(RegisterStoreModel model)
        {
            var newStore = new StoreModel
            {
                StoreId = Guid.NewGuid(),
                UserId = model.UserId,
                StoreName = $"Store_{Guid.NewGuid()}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _db.Add(newStore);
            await _db.SaveChangesAsync();

            return newStore.StoreId;

        }

        // Soft Delete
        public async Task<int> DeleteStoreAsync(Guid UserId)
        {
            var store = await _db.Stores.FirstOrDefaultAsync(s => s.UserId == UserId);
            if (store == null) 
            {
                return 0;
            }

            store.IsActive = false;
            store.UpdatedAt = DateTime.UtcNow;
            return await _db.SaveChangesAsync();

        }

        public async Task<StoreModel> GetStoreInfo(Guid userId)
        {
            var store = await _db.Stores.
                Include(s => s.AccountBanking)
                .FirstOrDefaultAsync(
                s => s.UserId == userId);

            if (store == null)
            {
                return null;
            }

            return store;
        }

        public Task<StoreModel> GetStoreByKeyword(string keyword)
        {
            throw new NotImplementedException();
        }

        // ✅ New: Get store detail by StoreId
        public async Task<StoreModel?> GetStoreDetailById(Guid storeId)
        {
            var store = await _db.Stores
                .Include(s => s.AccountBanking)
                .FirstOrDefaultAsync(s => s.StoreId == storeId);
            if (store == null) return null;

            return store;
           
        }

      

        // ✅ New: Get all active stores
        public async Task<IEnumerable<StoreDTO>?> GetActiveStoresAsync(int page, int pageSize)
        {
            // Validate input parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100; // Limit max page size to prevent performance issues

            return await _db.Stores
                .Where(s => s.IsActive)
                .OrderBy(s => s.CreatedAt) // Add ordering for consistent pagination
                .Skip((page - 1) * pageSize) // Skip records for previous pages
                .Take(pageSize) // Take only the required number of records
                .Select(s => new StoreDTO
                {
                    StoreId = s.StoreId,
                    StoreName = s.StoreName,
                    StoreCategory = s.StoreCategory,
                    Description = s.Description,
                    StoreImage = s.StoreImage,
                    IsActive = s.IsActive,
                    Address = s.Address,
                    Phone = s.Phone
                })
                .ToListAsync();
        }

        public async Task<int> GetActiveStoresCountAsync()
        {
            return await _db.Stores
                .Where(s => s.IsActive)
                .CountAsync();
        }

        public async Task<int> ChangeActiveStoreAsync(Guid storeId, bool IsActive)
        {
            var store = await _db.Stores.FirstOrDefaultAsync(
                s => s.StoreId == storeId);

            store.IsActive = IsActive;
            store.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
        public async Task<int> UpsertAccountBankingAsync(Guid storeId, string bankName, string accountNumber)
        {
            var store = await _db.Stores.AsNoTracking().FirstOrDefaultAsync(s => s.StoreId == storeId);
            if (store == null) return 0;

            var banking = await _db.AccountBankings.FirstOrDefaultAsync(b => b.StoreId == storeId);

            if (banking == null)
            {
                banking = new AccountBanking
                {
                    StoreId = storeId,
                    BankName = bankName?.Trim() ?? string.Empty,
                    AccountNumber = accountNumber?.Trim() ?? string.Empty,
                    Balance = 0m
                };
                _db.AccountBankings.Add(banking);
            }
            else
            {
                banking.BankName = bankName?.Trim() ?? banking.BankName;
                banking.AccountNumber = accountNumber?.Trim() ?? banking.AccountNumber;
                _db.AccountBankings.Update(banking);
            }

            // cập nhật UpdatedAt của store
            var trackedStore = await _db.Stores.FirstOrDefaultAsync(s => s.StoreId == storeId);
            if (trackedStore != null) trackedStore.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }

        public async Task<int> SetAccountBalanceAsync(Guid storeId, decimal newBalance)
        {
            if (newBalance < 0) throw new ArgumentOutOfRangeException(nameof(newBalance));

            var banking = await _db.AccountBankings.FirstOrDefaultAsync(b => b.StoreId == storeId);
            if (banking == null) return 0;

            banking.Balance = newBalance;

            var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreId == storeId);
            if (store != null) store.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }

        // ➕/➖ Cộng/trừ số dư an toàn (không cho âm)
        public async Task<int> IncreaseBalanceAsync(Guid storeId, decimal amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

            var banking = await _db.AccountBankings.FirstOrDefaultAsync(b => b.StoreId == storeId);
            if (banking == null) return 0;

            banking.Balance += amount;

            var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreId == storeId);
            if (store != null) store.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }

        public async Task<int> DecreaseBalanceAsync(Guid storeId, decimal amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

            var banking = await _db.AccountBankings.FirstOrDefaultAsync(b => b.StoreId == storeId);
            if (banking == null) return 0;

            if (banking.Balance - amount < 0)
                throw new InvalidOperationException("Balance cannot be negative.");

            banking.Balance -= amount;

            var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreId == storeId);
            if (store != null) store.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }

        // ❌ Xóa thông tin ngân hàng của Store (không xóa Store)
        public async Task<int> DeleteAccountBankingAsync(Guid storeId)
        {
            var banking = await _db.AccountBankings.FirstOrDefaultAsync(b => b.StoreId == storeId);
            if (banking == null) return 0;

            _db.AccountBankings.Remove(banking);

            var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreId == storeId);
            if (store != null) store.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }
    }
}

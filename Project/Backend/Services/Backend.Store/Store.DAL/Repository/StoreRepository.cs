using Microsoft.EntityFrameworkCore;
using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using Store.DAL.Databases;
using Store.DAL.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Store.DAL.Repository
{
    public class StoreRepository : IStoreRepository
    {
        private readonly StoreDbContext _db;

        public StoreRepository(StoreDbContext db)
        {

            _db = db;
        }

        public async Task<IEnumerable<StoreModel>> SearchStoreByKeywordAsync(string keyword)
        {
            return await _db.Stores
                .Where(p => (p.StoreName ?? "").Contains(keyword)).ToListAsync();
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
            var store = await _db.Stores.FirstOrDefaultAsync(
                s => s.UserId == userId);

            if (store == null)
            {
                return null;
            }

            //var info = new StoreDTO
            //{
            //    StoreId = store.StoreId,
            //    StoreName = store.StoreName,
            //    StoreCategory = store.StoreCategory,
            //    Description = store.Description,
            //    StoreImage = store.StoreImage,
            //    IsActive = store.IsActive,
            //    Address = store.Address,
            //    Phone = store.Phone
                
            //};

            return store;
        }

        public Task<StoreModel> GetStoreByKeyword(string keyword)
        {
            throw new NotImplementedException();
        }

        // ✅ New: Get store detail by StoreId
        public async Task<StoreModel?> GetStoreDetailById(Guid storeId)
        {
            var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreId == storeId);
            if (store == null) return null;

            return store;
            //return new StoreDTO
            //{
            //    StoreId = store.StoreId,
            //    StoreName = store.StoreName,
            //    StoreCategory = store.StoreCategory,
            //    Description = store.Description,
            //    StoreImage = store.StoreImage,
            //    IsActive = store.IsActive,
            //    Address = store.Address,
            //    Phone = store.Phone
            //};
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
    }
}

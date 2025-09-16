using Store.DAL.Databases;
using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using Store.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Store.DAL.Repository
{
    public class StoreRepository : IStoreRepository
    {
        private readonly StoreDbContext _db;

        public StoreRepository(StoreDbContext db)
        {

            _db = db;
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


        public async Task<int> UpdateStoreAsync(UpdateStoreModel model)
        {
            var store = await _db.Stores
                .FirstOrDefaultAsync(s => s.UserId == model.UserId);
            if (store == null) return 0;

            store.StoreName = model.StoreName;
            store.Description = model.Description;
            store.Address = model.Address;
            store.Phone = model.Phone;
            store.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync();



        }

        public async Task<StoreDTO> GetStoreInfo(Guid userId)
        {
            var store = await _db.Stores.FirstOrDefaultAsync(
                s => s.UserId == userId);

            if (store == null)
            {
                return null;
            }

            var info = new StoreDTO
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                Description = store.Description,
                IsActive = store.IsActive,
                Address = store.Address,
                Phone = store.Phone
                
            };

            return info;
        }

        public Task<StoreDTO> GetStoreByKeyword(string keyword)
        {
            throw new NotImplementedException();
        }

        // ✅ New: Get store detail by StoreId
        public async Task<StoreDTO?> GetStoreDetailById(Guid storeId)
        {
            var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreId == storeId);
            if (store == null) return null;

            return new StoreDTO
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                Description = store.Description,
                IsActive = store.IsActive,
                Address = store.Address,
                Phone = store.Phone
            };
        }

        //public async Task<StoreDTO?> BuyerGetStoreDetailById(Guid storeId)
        //{
        //    var store = await _db.Stores.FirstOrDefaultAsync(s => s.StoreId == storeId && s.IsActive);
        //    if (store == null) return null;

        //    return new StoreDTO
        //    {
        //        StoreId = store.StoreId,
        //        StoreName = store.StoreName,
        //        Description = store.Description,
        //        IsActive = store.IsActive,
        //        Address = store.Address,
        //        Phone = store.Phone
        //    };
        //}

        // ✅ New: Get all active stores
        public async Task<IEnumerable<StoreDTO>?> GetAllActiveStoresAsync()
        {
            return await _db.Stores
                .Where(s => s.IsActive)
                .Select(s => new StoreDTO
                {
                    StoreId = s.StoreId,
                    StoreName = s.StoreName,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    Address = s.Address,
                    Phone = s.Phone
                    
                })
                .ToListAsync();
        }


        public async Task<int> StoreActiveAsync(Guid userId, bool IsActive)
        {
            var store = await _db.Stores.FirstOrDefaultAsync(
                s => s.UserId == userId);

            store.IsActive = IsActive;
            store.UpdatedAt = DateTime.UtcNow;

            return await _db.SaveChangesAsync();
        }
    }
}

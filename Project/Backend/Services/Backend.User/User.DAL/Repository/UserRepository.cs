using User.DAL.Databases;
using User.DAL.Models.Entities;
using User.DAL.Repository.Interfaces;
using User.Common.Models.Requests;
using Microsoft.EntityFrameworkCore;
using static User.Common.Models.DTOs;

namespace User.DAL.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _db;

        public UserRepository(UserDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> CheckUserAsync(UserCheckModel user)
        {
            var existed = await _db.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existed != null) return existed.UserId;
            
            return Guid.Empty;
        }

        public async Task<Guid?> CreateUserAsync(UserCheckModel user, Guid? storeId)
        {

            var newUser = new UserModel
            {
                UserId = Guid.NewGuid(),
                StoreId = storeId,
                Email = user.Email,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            return newUser.UserId;
        }

        public async Task<int> DeleteUserAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return 0;

            _db.Users.Remove(user);
            return await _db.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetStoreIdsByUserIdAsync(Guid userId)
        {
            var stores = await _db.Users
                .Where(u => u.UserId == userId && u.StoreId != null)
                .Select(u => u.StoreId!.Value)
                .ToListAsync();
            return stores;
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return null;

            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                StoreId = user.StoreId,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDto?> UpdateUserAsync(UserUpdateModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null) return null;

            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;
            user.StoreId = model.StoreId;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                StoreId = user.StoreId,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

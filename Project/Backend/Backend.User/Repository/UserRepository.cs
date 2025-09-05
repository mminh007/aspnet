using Backend.User.Databases;
using Backend.User.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.User.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _db;

        public UserRepository(UserDbContext db)
        {
            _db = db;
        }

        public async Task<(bool Success, Guid UserId, string? ErrorMessage)> CreateUserAsync(UserCheckModel user)
        {
            try
            {
                var existed = await _db.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existed != null)
                    return (false, Guid.Empty, "Email already exists");

                var newUser = new UserModel
                {
                    UserId = Guid.NewGuid(),
                    Email = user.Email,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                return (true, newUser.UserId, null);
            }
            catch (Exception ex)
            {
                return (false, Guid.Empty, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    return (false, "User not found");

                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, UserUpdateModel? UserInfo)> GetUserByIdAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return (false, "User does not exist", null);

            return (true, null, new UserUpdateModel
            {
                UserId = user.UserId,
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            });
        }

        public async Task<(bool Success, string? ErrorMessage, UserUpdateModel? UserInfo)> UpdateUserAsync(UserUpdateModel model)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
                if (user == null)
                    return (false, "User not found", null);

                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;

                _db.Users.Update(user);
                await _db.SaveChangesAsync();

                return (true, null, new UserUpdateModel
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber
                });
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
    }
}

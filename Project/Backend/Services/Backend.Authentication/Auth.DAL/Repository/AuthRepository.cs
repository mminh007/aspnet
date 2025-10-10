using Auth.DAL.Databases;
using Auth.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.DAL.Repository.Interfaces
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AuthDbContext _db;

        public AuthRepository(AuthDbContext db)
        {
            _db = db;
        }

        public async Task<IdentityModel> AuthenticateAsynce(string Email)
        {
            return await _db.IdentityModels.FirstOrDefaultAsync(u => u.Email == Email);
        }

        public async Task<Guid> CreateIdentityAsync(IdentityModel model)
        {
            await _db.IdentityModels.AddAsync(model);
            await _db.SaveChangesAsync();
            return model.Id;
        }

        public async Task<int> UpdateIdentityAsync(IdentityModel model)
        {
            _db.IdentityModels.Update(model);
            return await _db.SaveChangesAsync();
        }

        public async Task<Guid> CreateRefreshTokenAsync(RefreshTokenModel entity)
        {
            await _db.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity.RefreshTokenId;
        }

        public async Task<int> UpdateRefreshTokenAsync(RefreshTokenModel entity)
        {
            _db.RefreshTokenModels.Update(entity);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> DeleteRefreshTokenAsync(RefreshTokenModel entity)
        {
            _db.RefreshTokenModels.Remove(entity);
            return await _db.SaveChangesAsync();
        }

        public async Task<RefreshTokenModel?> GetRefreshTokenAsync(byte[] hashingToken)
        {
            return await _db.RefreshTokenModels
                .FirstOrDefaultAsync(rt => rt.TokenHash.SequenceEqual(hashingToken));
        }

        public async Task<IdentityModel?> FindUserByTokenIdAsync(Guid identityId)
        {
            return await _db.IdentityModels.FirstOrDefaultAsync(u => u.Id == identityId);
        }

        public async Task<IdentityModel?> GetByEmailAsync(string email)
        {
            return await _db.IdentityModels.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<int> UpdateVerificationAsync(IdentityModel user)
        {
            _db.IdentityModels.Update(user);
            return await _db.SaveChangesAsync();
        }
    }
}

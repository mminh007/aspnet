using Auth.DAL.Models.Entities;

namespace Auth.DAL.Repository.Interfaces
{
    public interface IAuthRepository
    {
        Task<Guid> CreateIdentityAsync(IdentityModel model); // return => userId

        Task<IdentityModel> AuthenticateAsynce(string Email); // return new Entity

        Task<int> UpdateIdentityAsync(IdentityModel model); // return => row update

        Task<Guid> CreateRefreshTokenAsync(RefreshTokenModel entity);
        Task<int> UpdateRefreshTokenAsync(RefreshTokenModel entity);
        Task<int> DeleteRefreshTokenAsync(RefreshTokenModel entity);

        Task<RefreshTokenModel?> GetRefreshTokenAsync(byte[] hashingToken);
        Task<IdentityModel?> FindUserByTokenIdAsync(Guid tokenId);

        Task<int> UpdateVerificationAsync(IdentityModel user);
        Task<IdentityModel?> GetByEmailAsync(string email);

    }
}

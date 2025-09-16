namespace Adminstrator.HttpsClients.Interfaces
{
    public interface IUserApiClient
    {
        Task<(bool Success, string? Message, int statusCode, List<Guid>)> GetStoreByUserIdAsync(Guid userId);
        //Task<UserApiResponse<UserDto>> GetCurrentUserAsync();
        //Task<UserApiResponse<UserDto>> UpdateAsync(UserUpdateModel model);
        //Task<UserApiResponse<Guid>> DeleteAsync();
    }
}

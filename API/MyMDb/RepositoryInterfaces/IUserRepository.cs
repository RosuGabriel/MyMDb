using MyMDb.Models;

namespace MyMDb.RepositoryInterfaces
{
    public interface IUserRepository
    {
        Task<UserProfile> AddProfileAsync(UserProfile profile);
        Task<UserProfile?> GetProfileByUserIdAsync(string Id);
        Task<UserProfile?> UpdateProfileAsync(UserProfile profile);
    }
}

using MyMDb.Models;

namespace MyMDb.RepositoryInterfaces
{
    public interface IUserRepository
    {
        Task<UserProfile?> GetProfileByIdAsync(string Id);
    }
}

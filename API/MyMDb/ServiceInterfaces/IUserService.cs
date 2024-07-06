using MyMDb.DTOs;
using MyMDb.Models;
using MyMDb.RepositoryInterfaces;

namespace MyMDb.ServiceInterfaces
{
    public interface IUserService
    {
        // profile operations
        public Task<UserProfile?> CreateUserProfileAsync(UserProfile profile);
        public Task<UserProfile?> GetUserProfileAsync(string userId);
        public Task<UserProfile?> EditUserProfileAsync(string userId, UserProfile editedProfile);
    }
}

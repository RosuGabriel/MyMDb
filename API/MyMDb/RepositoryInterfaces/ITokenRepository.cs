using MyMDb.Models;

namespace MyMDb.RepositoryInterfaces
{
    public interface ITokenRepository : IRepository<UserRefreshToken>
    {
        public Task<UserRefreshToken?> GetRefreshTokenAsync(string userId, string refreshToken);
    }
}

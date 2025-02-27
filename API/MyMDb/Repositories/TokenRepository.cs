using MyMDb.RepositoryInterfaces;
using MyMDb.Models;
using MyMDb.Data;
using Microsoft.EntityFrameworkCore;

namespace MyMDb.Repositories
{
    public class TokenRepository : Repository<UserRefreshToken>, ITokenRepository
    {
        public TokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<UserRefreshToken?> GetRefreshTokenAsync(string userId, string refreshToken)
        {
            var token = await _dbSet.FirstOrDefaultAsync(t => t.UserId == userId && t.RefreshToken == refreshToken);
            
            return token;
        }
    }
}

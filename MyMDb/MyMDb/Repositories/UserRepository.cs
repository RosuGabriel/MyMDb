using Microsoft.EntityFrameworkCore;
using MyMDb.Data;
using MyMDb.Models;
using MyMDb.RepositoryInterfaces;

namespace MyMDb.Repositories
{
    public class UserRepository : Repository<AppUser>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<UserProfile?> GetProfileByIdAsync(string Id)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == Id);
        }
    }
}
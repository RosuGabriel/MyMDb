using MyMDb.Data;
using MyMDb.Models;
using MyMDb.RepositoryInterfaces;

namespace MyMDb.Repositories
{
    public class ProfileRepository : Repository<UserProfile>, IProfileRepository
    {
        public ProfileRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}

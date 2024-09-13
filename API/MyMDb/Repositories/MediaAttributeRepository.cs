using MyMDb.Data;
using MyMDb.Models;
using MyMDb.RepositoryInterfaces;

namespace MyMDb.Repositories
{
    public class MediaAttributeRepository : Repository<MediaAttribute>, IMediaAttributeRepository
    {
        public MediaAttributeRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}

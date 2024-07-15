using Microsoft.Identity.Client;
using MyMDb.Data;
using MyMDb.Models;
using MyMDb.RepositoryInterfaces;

namespace MyMDb.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}

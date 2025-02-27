using Microsoft.EntityFrameworkCore;
using MyMDb.Data;
using MyMDb.Models;
using MyMDb.RepositoryInterfaces;

namespace MyMDb.Repositories
{
    public class ContinueWatchingRepository : Repository<ContinueWatching>, IContinueWatchingRepository
    {
        public ContinueWatchingRepository(ApplicationDbContext context) : base(context)
        {

        }

        // For episodes of the same series, only the latest watched episode is shown in the Continue Watching list
        public async Task<ICollection<ContinueWatching>> GetAllByUserIdAsync(string userId)
        {
            var continueWatchings = await _dbSet
                .Where(cw => cw.UserId == userId)
                .Include(cw => cw.Media)
                .GroupBy(cw => cw.MediaId)
                .Select(group => group.OrderByDescending(c => c.DateModified).FirstOrDefault()!)
                .ToListAsync();
            foreach (var cw in continueWatchings)
            {
                if (cw.Media is Series series)
                {
                    await _context.Entry(series)
                        .Collection(s => s.Episodes!)
                        .LoadAsync();
                }
            }
            return continueWatchings;
        }
        
        public async Task<ContinueWatching?> GetByUserIdAndMediaIdAsync(string userId, Guid? mediaId, Guid? episodeId)
        {
            return await _context.ContinueWatchings
                .FirstOrDefaultAsync(cw => cw.UserId == userId && cw.MediaId == mediaId && cw.EpisodeId == episodeId);
        }
    }
}

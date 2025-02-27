using MyMDb.Models;

namespace MyMDb.RepositoryInterfaces
{
    public interface IContinueWatchingRepository : IRepository<ContinueWatching>
    {
        Task<ContinueWatching?> GetByUserIdAndMediaIdAsync(string userId, Guid? mediaId, Guid? episodeId);
        Task<ICollection<ContinueWatching>> GetAllByUserIdAsync(string userId);
    }
}

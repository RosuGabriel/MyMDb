using MyMDb.Models;

namespace MyMDb.ServiceInterfaces
{
    public interface IContinueWatchingService
    {
        Task<ICollection<ContinueWatching>> GetAllByUserIdAsync(string userId);
        Task<ContinueWatching?> GetByUserIdAndMediaIdAsync(string userId, Guid? mediaId, Guid? episodeId);
        Task<ContinueWatching?> AddOrUpdateAsync(string userId, Guid? mediaId, Guid? episodeId, int? secondsWatched, int? duration);
        Task DeleteAsync(string userId, Guid? mediaId, Guid? episodeId);
    }
}

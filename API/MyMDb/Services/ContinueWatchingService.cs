using MyMDb.RepositoryInterfaces;
using MyMDb.ServiceInterfaces;
using MyMDb.Models;
using Microsoft.AspNetCore.Identity;
using MyMDb.Data;
using Microsoft.IdentityModel.Tokens;

namespace MyMDb.Services
{
    public class ContinueWatchingService : IContinueWatchingService
    {
        private readonly IContinueWatchingRepository _continueWatchingRepository;
        private readonly UserManager<AppUser> _userManager;

        public ContinueWatchingService(IContinueWatchingRepository continueWatchingRepository, UserManager<AppUser> userManager)
        {
            _continueWatchingRepository = continueWatchingRepository;
            _userManager = userManager;
        }

        public async Task<ContinueWatching?> AddOrUpdateAsync(string userId, Guid? mediaId, Guid? episodeId, int? secondsWatched, int? duration)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ActionResponseExceptions.NotFoundException("User not found!");
            }

            var continueWatching = await GetByUserIdAndMediaIdAsync(userId, mediaId, episodeId);
            if (continueWatching == null)
            {
                continueWatching = new ContinueWatching
                {
                    UserId = userId,
                    MediaId = mediaId,
                    EpisodeId = episodeId,
                    WatchedTime = secondsWatched,
                    Duration = duration
                };

                continueWatching.Initialize();
                return await _continueWatchingRepository.AddAsync(continueWatching);
            }
            else
            {
                continueWatching.WatchedTime = secondsWatched;
                continueWatching.Duration = duration;

                continueWatching.UpdateDateModified();
                await _continueWatchingRepository.UpdateAsync(continueWatching);
            }

            return null;
        }

        public async Task DeleteAsync(string userId, Guid? mediaId, Guid? episodeId)
        {
            var continueWatching = await GetByUserIdAndMediaIdAsync(userId, mediaId, episodeId);
            if (continueWatching == null)
            {
                throw new ActionResponseExceptions.NotFoundException("Continue watching not found!");
            }

            await _continueWatchingRepository.DeleteAsync(continueWatching);
            
            return;
        }

        public async Task<ICollection<ContinueWatching>> GetAllByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ActionResponseExceptions.NotFoundException("User not found!");
            }

            var continueWatchings = await _continueWatchingRepository.GetAllByUserIdAsync(userId);
            var episodes = continueWatchings
                            .Where(cw => cw.EpisodeId.HasValue && cw.Media is Series)
                            .Select(cw => new ContinueWatching
                            {
                                MediaId = cw.MediaId,
                                WatchedTime = cw.WatchedTime,
                                Duration = cw.Duration,
                                Media = cw.Media,
                                EpisodeId = cw.EpisodeId,
                                EpisodeNumber = (cw.Media as Series)!.Episodes!
                                                .FirstOrDefault(e => e.Id == cw.EpisodeId)?.EpisodeNumber,
                                SeasonNumber = (cw.Media as Series)!.Episodes!
                                                .FirstOrDefault(e => e.Id == cw.EpisodeId)?.SeasonNumber,
                                PosterPath = cw.Media?.PosterPath
                            })
                            .ToList();

            var movies = continueWatchings
                        .Where(cw => !cw.EpisodeId.HasValue)
                        .Select(cw => new ContinueWatching
                        {
                            MediaId = cw.MediaId,
                            WatchedTime = cw.WatchedTime,
                            Duration = cw.Duration,
                            Media = cw.Media,
                            PosterPath = cw.Media?.PosterPath
                        })
                        .ToList();

            continueWatchings = episodes.Concat(movies).OrderByDescending(cw => cw.DateModified).ToList();
            return continueWatchings;
        }

        public async Task<ContinueWatching?> GetByUserIdAndMediaIdAsync(string userId, Guid? mediaId, Guid? episodeId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ActionResponseExceptions.NotFoundException("User not found!");
            }

            var continueWatching = await _continueWatchingRepository.GetByUserIdAndMediaIdAsync(userId, mediaId, episodeId);

            return continueWatching;
        }
    }
}

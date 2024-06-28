using MyMDb.RepositoryInterfaces;
using MyMDb.Models;
using MyMDb.ServiceInterfaces;

namespace MyMDb.Services
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _MediaRepository;

        public MediaService(IMediaRepository MediaRepository)
        {
            _MediaRepository = MediaRepository;
        }

        // getting all
        public async Task<ICollection<Media>> GetAllMedia()
        {
            return await _MediaRepository.GetAllMediaAsync();
        }

        public async Task<ICollection<Movie>> GetAllMovies()
        {
            return await _MediaRepository.GetAllMoviesAsync();
        }

        public async Task<ICollection<Series>> GetAllSeries()
        {
            return await _MediaRepository.GetAllSeriesAsync();
        }

        public async Task<ICollection<Episode>> GetEpisodesOfASeries(Guid seriesId)
        {
            return await _MediaRepository.GetEpisodesBySeriesIdAsync(seriesId);
        }

        // getting one
        public async Task<Media> GetMedia(Guid id)
        {
            return await _MediaRepository.GetByIdAsync(id);
        }

        // adding
        public async Task<Movie> AddMovie(string? title, string? description, DateTime? releaseDate, string? posterPath, string? videoPath)
        {
            var newMovie = new Movie();

            newMovie.Title = title;
            newMovie.Description = description;
            newMovie.ReleaseDate = releaseDate;
            newMovie.PosterPath = posterPath;
            newMovie.VideoPath = videoPath;

            return await _MediaRepository.CreateMovieAsync(newMovie);
        }

        public async Task<Series> AddSeries(string? title, string? description, DateTime? releaseDate, string? posterPath)
        {
            var newSeries = new Series();

            newSeries.Title = title;
            newSeries.Description = description;
            newSeries.ReleaseDate = releaseDate;
            newSeries.PosterPath = posterPath;

            return await _MediaRepository.CreateSeriesAsync(newSeries);
        }

        public async Task<Episode> AddEpisode(string? title, string? description, DateTime? releaseDate, string? posterPath, string? videoPath, int seasonNumber, Guid seriesId, int? episodeNumber)
        {
            var newEpisode = new Episode();

            newEpisode.Title = title;
            newEpisode.Description = description;
            newEpisode.ReleaseDate = releaseDate;
            newEpisode.PosterPath = posterPath;
            newEpisode.VideoPath = videoPath;

            newEpisode.SeasonNumber = seasonNumber;
            newEpisode.SeriesId = seriesId;

            if (episodeNumber != null)
            {
                newEpisode.EpisodeNumber = episodeNumber;
            }
            else
            {
                newEpisode.EpisodeNumber = await _MediaRepository.GetLastEpisodeOfASeasonAsync(seriesId, seasonNumber) + 1;
            }

            return await _MediaRepository.CreateEpisodeAsync(newEpisode);
        }

        public async Task<ICollection<Episode>> AddManyEpisodesToASeries(Guid seriesId, int seasonNumber, int episodesNumber)
        {
            var lastEpisodeNumber = await _MediaRepository.GetLastEpisodeOfASeasonAsync(seriesId, seasonNumber);
            var newEpisodes = new List<Episode>();
            var series = await _MediaRepository.GetByIdAsync(seriesId);

            for(var episodeNumber = 1;  episodeNumber <= episodesNumber; episodeNumber++)
            {
                var newEpisode = new Episode();
                newEpisode.SeriesId = seriesId;
                newEpisode.SeasonNumber = seasonNumber;
                newEpisode.EpisodeNumber = lastEpisodeNumber + episodeNumber;
                newEpisode.PosterPath = series.PosterPath;

                newEpisode = await _MediaRepository.CreateEpisodeAsync(newEpisode);
                newEpisodes.Add(newEpisode);
            }

            return newEpisodes;
        }
    }
}

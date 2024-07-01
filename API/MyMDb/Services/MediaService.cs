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

        // getting
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

        public async Task<Media?> GetById(Guid id)
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

            newMovie.Initialize();

            return await _MediaRepository.CreateMovieAsync(newMovie);
        }

        public async Task<Series> AddSeries(string? title, string? description, DateTime? releaseDate, string? posterPath)
        {
            var newSeries = new Series();

            newSeries.Title = title;
            newSeries.Description = description;
            newSeries.ReleaseDate = releaseDate;
            newSeries.PosterPath = posterPath;

            newSeries.Initialize();

            return await _MediaRepository.CreateSeriesAsync(newSeries);
        }

        public async Task<Episode> AddEpisode(string? title, string? description, DateTime? releaseDate, int seasonNumber, Guid seriesId, int? episodeNumber, string? posterPath, string? videoPath)
        {
            var newEpisode = new Episode();

            newEpisode.Title = title;
            newEpisode.Description = description;
            newEpisode.ReleaseDate = releaseDate;
            newEpisode.PosterPath = posterPath;
            newEpisode.VideoPath = videoPath;

            newEpisode.SeasonNumber = seasonNumber;
            newEpisode.SeriesId = seriesId;

            newEpisode.Initialize();

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
                
                newEpisode.Initialize();

                newEpisode = await _MediaRepository.CreateEpisodeAsync(newEpisode);
                
                newEpisodes.Add(newEpisode);
            }

            return newEpisodes;
        }

        public async Task<Movie?> EditMovie(Guid id, Movie editedMovie)
        {
            var movieToEdit = await _MediaRepository.GetMovieByIdAsync(id);

            if (movieToEdit == null) 
            {
                return null;
            }

            movieToEdit = editedMovie;

            movieToEdit.UpdateDateModified();

            await _MediaRepository.UpdateMovie(movieToEdit);

            return movieToEdit;
        }

        public async Task<Series?> EditSeries(Guid id, Series editedSeries)
        {
            var seriesToEdit = await _MediaRepository.GetSeriesByIdAsync(id);

            if (seriesToEdit == null)
            {
                return null;
            }

            seriesToEdit = editedSeries;

            seriesToEdit.UpdateDateModified();

            await _MediaRepository.UpdateSeries(seriesToEdit);

            return seriesToEdit;
        }

        public async Task<Episode?> EditEpisode(Guid id, Episode editedEpisode)
        {
            var episodeToEdit = await _MediaRepository.GetEpisodeByIdAsync(id);

            if (episodeToEdit == null)
            {
                return null;
            }

            episodeToEdit = editedEpisode;

            episodeToEdit.UpdateDateModified();

            await _MediaRepository.UpdateEpisode(episodeToEdit);

            return episodeToEdit;
        }

        public async Task<bool> DeleteMedia(Guid id)
        {
            var mediaToDelete = await _MediaRepository.GetByIdAsync(id);
            
            if (mediaToDelete == null)
            {
                return false;
            }
            
            await _MediaRepository.Delete(mediaToDelete);
            return true;
        }

        public async Task<Movie?> GetMovieById(Guid id)
        {
            return await _MediaRepository.GetMovieByIdAsync(id);
        }

        public async Task<Series?> GetSeriesById(Guid id)
        {
            return await _MediaRepository.GetSeriesByIdAsync(id);
        }

        public async Task<Episode?> GetEpisodeById(Guid id)
        {
            return await _MediaRepository.GetEpisodeByIdAsync(id);
        }
    }
}

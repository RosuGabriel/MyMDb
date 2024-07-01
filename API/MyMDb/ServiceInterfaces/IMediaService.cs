using MyMDb.Models;

namespace MyMDb.ServiceInterfaces
{
    public interface IMediaService
    {
        // getting all
        Task<ICollection<Media>> GetAllMedia();
        Task<ICollection<Movie>> GetAllMovies();
        Task<ICollection<Series>> GetAllSeries();
        Task<ICollection<Episode>> GetEpisodesOfASeries(Guid seriesId);

        // getting by id
        Task<Media?> GetById(Guid id);
        Task<Movie?> GetMovieById(Guid id);
        Task<Series?> GetSeriesById(Guid id);
        Task<Episode?> GetEpisodeById(Guid id);

        // adding
        Task<Movie> AddMovie(string? title, string? description, DateTime? releaseDate, string? posterPath, string? videoPath);
        Task<Series> AddSeries(string? title, string? description, DateTime? releaseDate, string? posterPath);
        Task<Episode> AddEpisode(string? title, string? description, DateTime? releaseDate, int seasonNumber, Guid seriesId, int? episodeNumber, string? posterPath, string? videoPath);
        Task<ICollection<Episode>> AddManyEpisodesToASeries(Guid seriesId, int seasonNumber, int episodesNumber);

        // editing
        Task<Movie?> EditMovie(Guid id, Movie editedMovie);
        Task<Series?> EditSeries(Guid id, Series editedSeries);
        Task<Episode?> EditEpisode(Guid id, Episode editedEpisode);

        //delete
        Task<bool> DeleteMedia(Guid id);
    }
}
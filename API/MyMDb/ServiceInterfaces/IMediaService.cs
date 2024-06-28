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

        // adding
        Task<Movie> AddMovie(string? title, string? description, DateTime? releaseDate, string? posterPath, string? videoPath);
        Task<Series> AddSeries(string? title, string? description, DateTime? releaseDate, string? posterPath);
        Task<Episode> AddEpisode(string? title, string? description, DateTime? releaseDate, string? posterPath, string? videoPath, int seasonNumber, Guid seriesId, int? episodeNumber);
        Task<ICollection<Episode>> AddManyEpisodesToASeries(Guid seriesId, int seasonNumber, int episodesNumber);
    }
}
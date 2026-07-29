using MyMDb.DTOs;
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
        Task<MovieDto> AddMovie(MovieDto movieDto, IFormFile? poster, IFormFile? video);
        Task<MovieDto> AddMovieStreamed(MovieDto movieDto, string? posterTempPath, string? videoFinalPath);
        Task<SeriesDto> AddSeries(SeriesDto seriesDto, IFormFile? poster);
        Task<EpisodeDto> AddEpisode(EpisodeDto episodeDto, IFormFile? poster, IFormFile? video);
        Task<EpisodeDto> AddEpisodeStreamed(EpisodeDto episodeDto, string? posterTempPath, string? videoFinalPath);
        Task<ICollection<Episode>> AddManyEpisodesToASeries(Guid seriesId, int seasonNumber, int episodesNumber, string? posterPath);

        // editing
        Task<MovieDto?> EditMovie(Guid id, MovieDto editedMovieDto, IFormFile? poster, IFormFile? video);
        Task<SeriesDto?> EditSeries(Guid id, SeriesDto editedSeriesDto, IFormFile? poster);
        Task<EpisodeDto?> EditEpisode(Guid id, EpisodeDto editedEpisodeDto, IFormFile? poster, IFormFile? video);

        // delete
        Task<bool> DeleteMedia(Guid id);

        // others
        public Task NormalizeVideo(string videoPath);
        Task<MediaAttribute> AddAttribute(MediaAttribute mediaAttribute);
        Task<MediaAttribute?> UpdateAttribute(MediaAttribute mediaAttribute, IFormFile? file);
        Task<bool> DeleteAttribute(Guid mediaId, string attributeType, string language);
        public string SanitizeFileName(string fileName);

        // streaming
        Task<VideoStreamInfoDto> GetVideoStreamInfoAsync(Guid mediaId);
    }
}
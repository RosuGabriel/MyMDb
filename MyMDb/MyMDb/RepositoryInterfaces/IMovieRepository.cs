using Microsoft.EntityFrameworkCore;
using MyMDb.Models;

namespace MyMDb.RepositoryInterfaces
{
    public interface IMovieRepository
    {
        Task<ICollection<Review>> GetReviewsByIdAsync(Guid Id);
        Task<double?> GetAverageReviewScoreAsync(Guid Id);
        Task<ICollection<Episode>> GetEpisodesBySeriesIdAsync(Guid seriesId);
    }
}

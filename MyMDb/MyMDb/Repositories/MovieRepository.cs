using Microsoft.EntityFrameworkCore;
using MyMDb.Data;
using MyMDb.Models;
using MyMDb.RepositoryInterfaces;

namespace MyMDb.Repositories
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        public MovieRepository (ApplicationDbContext context) : base(context) 
        {

        }

        public async Task<ICollection<Review>> GetReviewsByIdAsync(Guid Id)
        {
            return await _context.Reviews
                .Where(r => r.MovieId == Id)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();
        }

        public async Task<double?> GetAverageReviewScoreAsync(Guid Id)
        {
            var reviews = await _context.Reviews
                .Where(r => r.MovieId == Id)
                .ToListAsync();

            if (reviews.Count == 0)
                return null;

            return reviews.Average(r => r.Rating);
        }

        public async Task<ICollection<Episode>> GetEpisodesBySeriesIdAsync(Guid seriesId)
        {
            return await _context.Episodes
                .Where(e => e.SeriesId == seriesId)
                .OrderBy(e => e.SeasonNumber)
                .ThenBy(e =>  e.EpisodeNumber)
                .ToListAsync();
        }
    }
}

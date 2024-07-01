using MyMDb.Models.Base;

namespace MyMDb.DTOs
{
    public class MovieDto : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? PosterPath { get; set; }
        public string? VideoPath { get; set; }
    }
}

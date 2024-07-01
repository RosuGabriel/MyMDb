using MyMDb.Models.Base;

namespace MyMDb.DTOs
{
    public class EpisodeDto : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? PosterPath { get; set; }
        public string? VideoPath { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
    }
}

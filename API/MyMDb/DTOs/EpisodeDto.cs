using MyMDb.Models.Base;

namespace MyMDb.DTOs
{
    public class EpisodeDto : MediaDto
    {
        public string? VideoPath { get; set; }
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
    }
}

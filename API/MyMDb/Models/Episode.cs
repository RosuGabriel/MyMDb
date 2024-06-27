namespace MyMDb.Models
{
    public class Episode : Movie
    {
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public Guid? SeriesId { get; set; }
        public virtual Series? Series { get; set; }
    }
}

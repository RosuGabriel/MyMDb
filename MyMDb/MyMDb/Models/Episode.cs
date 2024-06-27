namespace MyMDb.Models
{
    public class Episode : Movie
    {
        public Guid? SeriesId { get; set; }
        public virtual Series? Series { get; set; }
    }
}

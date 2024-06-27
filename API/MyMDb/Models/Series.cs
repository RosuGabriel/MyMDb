namespace MyMDb.Models
{
    public class Series : Movie
    {
        public virtual ICollection<Episode>? Episodes { get; set; }
    }
}

namespace MyMDb.Models
{
    public class Series : Media
    {
        public virtual ICollection<Episode>? Episodes { get; set; }
    }
}

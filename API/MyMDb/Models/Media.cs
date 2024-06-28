using Microsoft.AspNetCore.Mvc.ViewEngines;
using MyMDb.Models.Base;

namespace MyMDb.Models
{
    public class Media : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string? PosterPath { get; set; }
        public string? VideoPath { get; set; }

        public virtual ICollection<Review>? Reviews { get; set; }
    }
}

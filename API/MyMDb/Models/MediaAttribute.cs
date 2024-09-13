using MyMDb.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace MyMDb.Models
{
    public class MediaAttribute : BaseEntity
    {
        [Required]
        public Guid MediaId { get; set; }
        public virtual Media? Media { get; }

        // subtitles or dubbing
        public string? Type { get; set; }
        public string? AttributePath { get; set; }
        public string? Language { get; set; }
    }
}

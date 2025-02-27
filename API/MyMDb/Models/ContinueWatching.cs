using MyMDb.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyMDb.Models
{
    public class ContinueWatching : BaseEntity
    {
        public Guid? MediaId { get; set; } // Movie or Series
        public Guid? EpisodeId { get; set; } // If Media is a Series
        public string? UserId { get; set; }
        public int? WatchedTime { get; set; }
        public int? Duration { get; set; }
        public virtual Media? Media { get; set; }
        [NotMapped]
        public virtual int? EpisodeNumber { get; set; }
        [NotMapped]
        public virtual int? SeasonNumber { get; set; }
        [NotMapped]
        public virtual string? PosterPath { get; set; }
    }
}

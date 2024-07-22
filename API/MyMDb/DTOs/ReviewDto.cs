using MyMDb.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace MyMDb.DTOs
{
    public class ReviewDto : BaseEntity
    {
        [Required]
        public Guid mediaId { get; set; }
        [Required]
        [Range(1.0, 10.0, ErrorMessage = "Rating must be between 1 and 10.")]
        public double? Rating { get; set; }
        public string? Comment { get; set; }
    }
}

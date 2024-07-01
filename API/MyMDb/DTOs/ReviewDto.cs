using MyMDb.Models.Base;

namespace MyMDb.DTOs
{
    public class ReviewDto : BaseEntity
    {
        public double? Rating { get; set; }
        public string? Comment { get; set; }
    }
}

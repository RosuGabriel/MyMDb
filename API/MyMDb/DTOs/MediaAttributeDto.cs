using MyMDb.Models.Base;

namespace MyMDb.DTOs
{
    public class MediaAttributeDto : BaseEntity
    {
        public Guid MediaId { get; set; }
        public string? Type { get; set; }
        public string? AttributePath { get; set; }
        public string? Language { get; set; }
    }
}

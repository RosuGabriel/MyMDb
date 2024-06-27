using MyMDb.Models.Base;

namespace MyMDb.Models
{
    public class UserProfile : BaseEntity
    {
        public string? ProfilePicPath { get; set; }
        public string? Description { get; set; }


        public string? UserId { get; set; }
        public virtual AppUser? User { get; set; }
    }
}

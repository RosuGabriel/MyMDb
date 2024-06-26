using Microsoft.AspNetCore.Identity;
using MyMDb.Models.Reviews;

namespace MyMDb.Models
{
    public class AppUser : IdentityUser
    {
        public virtual UserProfile? UserProfile { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MyMDb.DTOs
{
    public class ProfileDto
    {
        [Required]
        public string? UserId { get; set; }
        [Required]
        public string? UserName { get; set; }
        public string? ProfilePicPath { get; set; }
    }
}

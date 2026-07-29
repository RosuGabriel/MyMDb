namespace MyMDb.DTOs
{
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? ProfilePicPath { get; set; }
        public bool Approved { get; set; }
        public IList<string>? Roles { get; set; }
    }
}

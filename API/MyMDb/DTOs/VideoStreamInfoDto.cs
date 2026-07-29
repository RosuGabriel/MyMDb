namespace MyMDb.DTOs
{
    public class VideoStreamInfoDto
    {
        public string FullPath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    }
}

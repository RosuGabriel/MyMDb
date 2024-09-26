namespace MyMDb.ServiceInterfaces
{
    public interface IFileProcessingService
    {
        public Task ProcessFileAsync(IFormFile file, string filePath);
        public Task ProcessVideoFileAsync(IFormFile file, string filePath, IMediaService mediaService, int bufferSize);
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyMDb.ServiceInterfaces;

namespace MyMDb.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        public Task ProcessVideoFileAsync(IFormFile file, string filePath, IMediaService mediaService, int bufferSize)
        {
            return Task.Run(async () =>
            {
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, bufferSize, FileOptions.WriteThrough | FileOptions.Asynchronous))
                {
                    await file.CopyToAsync(stream);
                }

                await mediaService.NormalizeVideo(filePath);
            });
        }

        public Task ProcessFileAsync(IFormFile file, string filePath)
        {
            return Task.Run(async () =>
            {
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(stream);
                }
            });
        }
    }
}

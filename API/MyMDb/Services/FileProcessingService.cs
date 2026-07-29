using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyMDb.ServiceInterfaces;
using MyMDb.Data;

namespace MyMDb.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly ILogger<FileProcessingService> _logger;

        public FileProcessingService(ILogger<FileProcessingService> logger)
        {
            _logger = logger;
        }

        public Task ProcessVideoFileAsync(IFormFile file, string filePath, IMediaService mediaService, int bufferSize)
        {
            return Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Starting video file processing. File: {FileName}, Target path: {FilePath}", file.FileName, filePath);

                    // Validate inputs
                    if (file == null || file.Length == 0)
                    {
                        _logger.LogError("Invalid file provided for video processing. File is null or empty");
                        throw new ArgumentException("File is null or empty", nameof(file));
                    }

                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        _logger.LogError("Invalid file path provided for video processing");
                        throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
                    }

                    // Ensure directory exists
                    string? directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        try
                        {
                            _logger.LogInformation("Creating directory: {Directory}", directory);
                            Directory.CreateDirectory(directory);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to create directory: {Directory}", directory);
                            throw;
                        }
                    }

                    // Copy file with error handling
                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, bufferSize, FileOptions.WriteThrough | FileOptions.Asynchronous))
                    {
                        try
                        {
                            _logger.LogInformation("Copying file content to {FilePath}, Size: {FileSize} bytes", filePath, file.Length);
                            await file.CopyToAsync(stream);
                            _logger.LogInformation("Successfully copied file to {FilePath}", filePath);
                        }
                        catch (IOException ioEx)
                        {
                            _logger.LogError(ioEx, "IO error while copying file to {FilePath}. This may indicate insufficient disk space or permission issues", filePath);
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unexpected error while copying file to {FilePath}", filePath);
                            throw;
                        }
                    }

                    // Normalize video (convert format, codec, etc.)
                    try
                    {
                        _logger.LogInformation("Starting video normalization for {FilePath}", filePath);
                        await mediaService.NormalizeVideo(filePath);
                        _logger.LogInformation("Successfully normalized video at {FilePath}", filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during video normalization at {FilePath}. Video file was saved but may need manual conversion", filePath);
                        throw;
                    }
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    _logger.LogError(uaEx, "Access denied when processing video file at {FilePath}. Check file permissions", filePath);
                    throw new InvalidOperationException($"Access denied when processing video file. Check permissions for path: {filePath}", uaEx);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error during video file processing. File: {FileName}, Path: {FilePath}", file.FileName, filePath);
                    throw;
                }
            });
        }

        public Task ProcessFileAsync(IFormFile file, string filePath)
        {
            return Task.Run(async () =>
            {
                try
                {
                    _logger.LogInformation("Starting file processing. File: {FileName}, Target path: {FilePath}", file.FileName, filePath);

                    // Validate inputs
                    if (file == null || file.Length == 0)
                    {
                        _logger.LogError("Invalid file provided for processing. File is null or empty");
                        throw new ArgumentException("File is null or empty", nameof(file));
                    }

                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        _logger.LogError("Invalid file path provided for processing");
                        throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
                    }

                    // Ensure directory exists
                    string? directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        try
                        {
                            _logger.LogInformation("Creating directory: {Directory}", directory);
                            Directory.CreateDirectory(directory);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to create directory: {Directory}", directory);
                            throw;
                        }
                    }

                    // Copy file with error handling
                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        try
                        {
                            _logger.LogInformation("Copying file content to {FilePath}, Size: {FileSize} bytes", filePath, file.Length);
                            await file.CopyToAsync(stream);
                            _logger.LogInformation("Successfully processed and saved file to {FilePath}", filePath);
                        }
                        catch (IOException ioEx)
                        {
                            _logger.LogError(ioEx, "IO error while copying file to {FilePath}. This may indicate insufficient disk space or permission issues", filePath);
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Unexpected error while copying file to {FilePath}", filePath);
                            throw;
                        }
                    }
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    _logger.LogError(uaEx, "Access denied when processing file at {FilePath}. Check file permissions", filePath);
                    throw new InvalidOperationException($"Access denied when processing file. Check permissions for path: {filePath}", uaEx);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Critical error during file processing. File: {FileName}, Path: {FilePath}", file.FileName, filePath);
                    throw;
                }
            });
        }
    }
}

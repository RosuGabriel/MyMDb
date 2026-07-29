using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace MyMDb.Helpers
{
    public class StreamedFileInfo
    {
        public string FieldName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string TempFilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    public class MultipartFormData
    {
        public Dictionary<string, string> FormFields { get; set; } = new();
        public Dictionary<string, StreamedFileInfo> Files { get; set; } = new();
    }

    public static class StreamingHelpers
    {
        private static readonly string[] _permittedVideoExtensions = { ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".webm" };
        private static readonly string[] _permittedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

        /// <summary>
        /// Streams multipart form data directly to disk without buffering in memory.
        /// Videos are written directly to their final destination.
        /// Images are written to temp files (since they're small).
        /// </summary>
        public static async Task<MultipartFormData> StreamMultipartToDiskAsync(
            HttpRequest request,
            string videosBasePath,
            string imagesBasePath,
            string tempPath,
            int bufferSize,
            Func<string, string> sanitizeFileName,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var result = new MultipartFormData();

            if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
            {
                throw new InvalidOperationException("Request is not a multipart request.");
            }

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(request.ContentType),
                lengthLimit: 70);

            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync(cancellationToken);

            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader && contentDisposition != null)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var fieldName = contentDisposition.Name.Value ?? string.Empty;
                        var fileName = contentDisposition.FileName.Value ?? string.Empty;

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var fileInfo = await StreamFileAsync(
                                section,
                                fieldName,
                                fileName,
                                videosBasePath,
                                imagesBasePath,
                                tempPath,
                                bufferSize,
                                sanitizeFileName,
                                logger,
                                cancellationToken);

                            if (fileInfo != null)
                            {
                                result.Files[fieldName] = fileInfo;
                            }
                        }
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        var fieldName = contentDisposition.Name.Value ?? string.Empty;

                        // Read the form field value
                        var encoding = GetEncoding(section);
                        using var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true);

                        var value = await streamReader.ReadToEndAsync(cancellationToken);
                        result.FormFields[fieldName] = value;
                    }
                }

                section = await reader.ReadNextSectionAsync(cancellationToken);
            }

            return result;
        }

        private static async Task<StreamedFileInfo?> StreamFileAsync(
            MultipartSection section,
            string fieldName,
            string fileName,
            string videosBasePath,
            string imagesBasePath,
            string tempPath,
            int bufferSize,
            Func<string, string> sanitizeFileName,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            string targetPath;
            bool isVideo = _permittedVideoExtensions.Contains(extension);
            bool isImage = _permittedImageExtensions.Contains(extension);

            if (!isVideo && !isImage)
            {
                logger.LogWarning("Skipping file with unsupported extension: {Extension}", extension);
                // Drain the section to continue reading
                await section.Body.CopyToAsync(Stream.Null, cancellationToken);
                return null;
            }

            // Determine target path based on file type
            if (isVideo)
            {
                // Videos go directly to final destination
                targetPath = Path.Combine(videosBasePath, sanitizeFileName(fileName));
            }
            else
            {
                // Images go to temp first (they're small, and we might need posterPath from form)
                targetPath = Path.Combine(tempPath, $"{Guid.NewGuid()}{extension}");
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            logger.LogInformation("Streaming file {FileName} ({FieldName}) directly to {TargetPath}", fileName, fieldName, targetPath);

            long totalBytes = 0;
            await using (var targetStream = new FileStream(
                targetPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var buffer = new byte[bufferSize];
                int bytesRead;

                while ((bytesRead = await section.Body.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await targetStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalBytes += bytesRead;
                }
            }

            logger.LogInformation("Successfully streamed {Bytes} bytes to {TargetPath}", totalBytes, targetPath);

            return new StreamedFileInfo
            {
                FieldName = fieldName,
                FileName = fileName,
                TempFilePath = targetPath,
                FileSize = totalBytes
            };
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and shouldn't be honored
            if (!hasMediaTypeHeader || mediaType?.Encoding == null || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }
    }
}

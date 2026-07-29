using MyMDb.RepositoryInterfaces;
using MyMDb.Models;
using MyMDb.Data;
using MyMDb.ServiceInterfaces;
using System.Diagnostics;
using AutoMapper;
using MyMDb.DTOs;

namespace MyMDb.Services
{
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _MediaRepository;
        private readonly IMediaAttributeRepository _MediaAttributeRepository;
        private readonly IContinueWatchingService _ContinueWatchingService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IFileProcessingService _FileProcessingService;
        private readonly ILogger<MediaService> _logger;

        public MediaService(IMediaRepository MediaRepository, IConfiguration configuration, IMediaAttributeRepository mediaAttributeRepository, IContinueWatchingService continueWatchingService, IMapper mapper, IFileProcessingService fileProcessingService, ILogger<MediaService> logger)
        {
            _MediaRepository = MediaRepository;
            _configuration = configuration;
            _MediaAttributeRepository = mediaAttributeRepository;
            _ContinueWatchingService = continueWatchingService;
            _logger = logger;
            _FileProcessingService = fileProcessingService;
            _mapper = mapper;
        }

        // getting
        public async Task<ICollection<Media>> GetAllMedia()
        {
            return await _MediaRepository.GetAllMediaAsync();
        }

        public async Task<ICollection<Movie>> GetAllMovies()
        {
            return await _MediaRepository.GetAllMoviesAsync();
        }

        public async Task<ICollection<Series>> GetAllSeries()
        {
            return await _MediaRepository.GetAllSeriesAsync();
        }

        public async Task<ICollection<Episode>> GetEpisodesOfASeries(Guid seriesId)
        {
            return await _MediaRepository.GetEpisodesBySeriesIdAsync(seriesId);
        }

        public async Task<Media?> GetById(Guid id)
        {
            return await _MediaRepository.GetByIdAsync(id);
        }

        // adding
        public async Task<MovieDto> AddMovie(MovieDto movieDto, IFormFile? poster, IFormFile? video)
        {
            var newMovie = _mapper.Map<Movie>(movieDto);
            newMovie.Initialize();

            if (poster != null)
            {
                if (!Extensions.IsImageFile(poster.FileName))
                    throw new ArgumentException("Not an image file provided for poster.");
                if (newMovie.PosterPath == null)
                    throw new ArgumentException("No path provided for poster.");

                newMovie.PosterPath = _configuration["Paths:Images"] + SanitizeFileName(newMovie.PosterPath);
                await _FileProcessingService.ProcessFileAsync(poster, Path.Combine(_configuration["Paths:Root"]!, newMovie.PosterPath));
            }

            if (video != null)
            {
                if (!Extensions.IsVideoFile(video.FileName))
                    throw new ArgumentException("Not a video file provided for video.");
                if (newMovie.VideoPath == null)
                    throw new ArgumentException("No path provided for video.");

                newMovie.VideoPath = _configuration["Paths:Videos"] + SanitizeFileName(newMovie.VideoPath);
                int bufferSize = int.TryParse(_configuration["VideoBufferSize"], out var size) ? size : 10000;
                await _FileProcessingService.ProcessVideoFileAsync(video, Path.Combine(_configuration["Paths:Root"]!, newMovie.VideoPath), this, bufferSize);
            }

            newMovie = await _MediaRepository.CreateMovieAsync(newMovie);
            return _mapper.Map<MovieDto>(newMovie);
        }

        /// <summary>
        /// Add a movie using pre-streamed files (video already written to disk).
        /// </summary>
        public async Task<MovieDto> AddMovieStreamed(MovieDto movieDto, string? posterTempPath, string? videoFinalPath)
        {
            var newMovie = _mapper.Map<Movie>(movieDto);
            newMovie.Initialize();

            if (!string.IsNullOrEmpty(posterTempPath) && File.Exists(posterTempPath))
            {
                if (newMovie.PosterPath == null)
                    throw new ArgumentException("No path provided for poster.");

                var finalPosterPath = Path.Combine(
                    _configuration["Paths:Root"]!,
                    _configuration["Paths:Images"]!,
                    SanitizeFileName(newMovie.PosterPath));

                var posterDir = Path.GetDirectoryName(finalPosterPath);
                if (!string.IsNullOrEmpty(posterDir) && !Directory.Exists(posterDir))
                    Directory.CreateDirectory(posterDir);

                File.Move(posterTempPath, finalPosterPath, overwrite: true);
                newMovie.PosterPath = _configuration["Paths:Images"] + SanitizeFileName(newMovie.PosterPath);
            }

            if (!string.IsNullOrEmpty(videoFinalPath) && File.Exists(videoFinalPath))
            {
                // Video was already streamed to final location, just update the path
                var relativePath = Path.GetRelativePath(_configuration["Paths:Root"]!, videoFinalPath);
                newMovie.VideoPath = relativePath.Replace("\\", "/");

                // Normalize video in background
                _ = Task.Run(() => NormalizeVideo(videoFinalPath));
            }

            newMovie = await _MediaRepository.CreateMovieAsync(newMovie);
            return _mapper.Map<MovieDto>(newMovie);
        }

        public async Task<SeriesDto> AddSeries(SeriesDto seriesDto, IFormFile? poster)
        {
            var newSeries = _mapper.Map<Series>(seriesDto);
            newSeries.Initialize();

            var seriesImagesDirectory = Path.Combine(_configuration["Paths:Root"]!, _configuration["Paths:Images"]!, SanitizeFileName(newSeries.Title));
            if (!Directory.Exists(seriesImagesDirectory))
                Directory.CreateDirectory(seriesImagesDirectory);

            var seriesVideosDirectory = Path.Combine(_configuration["Paths:Root"]!, _configuration["Paths:Videos"]!, SanitizeFileName(newSeries.Title));
            if (!Directory.Exists(seriesVideosDirectory))
                Directory.CreateDirectory(seriesVideosDirectory);

            if (poster != null)
            {
                if (!Extensions.IsImageFile(poster.FileName))
                    throw new ArgumentException("Not an image file provided for poster.");
                if (newSeries.PosterPath == null)
                    throw new ArgumentException("No path provided for poster.");

                newSeries.PosterPath = SanitizeFileName(Path.Combine(_configuration["Paths:Images"]!, newSeries.Title, newSeries.PosterPath));
                await _FileProcessingService.ProcessFileAsync(poster, Path.Combine(_configuration["Paths:Root"]!, newSeries.PosterPath));
            }

            newSeries = await _MediaRepository.CreateSeriesAsync(newSeries);
            return _mapper.Map<SeriesDto>(newSeries);
        }

        public async Task<EpisodeDto> AddEpisode(EpisodeDto episodeDto, IFormFile? poster, IFormFile? video)
        {
            var newEpisode = _mapper.Map<Episode>(episodeDto);
            newEpisode.Initialize();

            var series = await _MediaRepository.GetSeriesByIdAsync(newEpisode.SeriesId);
            if (series == null)
                throw new ArgumentException("The episode is added to an non-existent series");

            if (newEpisode.EpisodeNumber == null)
                newEpisode.EpisodeNumber = await _MediaRepository.GetLastEpisodeOfASeasonAsync(newEpisode.SeriesId, newEpisode.SeasonNumber) + 1;

            if (poster != null)
            {
                if (!Extensions.IsImageFile(poster.FileName))
                    throw new ArgumentException("Not an image file provided for poster.");
                if (newEpisode.PosterPath == null)
                    throw new ArgumentException("No path provided for poster.");

                newEpisode.PosterPath = SanitizeFileName(Path.Combine(_configuration["Paths:Images"]!, series.Title, newEpisode.PosterPath));
                await _FileProcessingService.ProcessFileAsync(poster, Path.Combine(_configuration["Paths:Root"]!, newEpisode.PosterPath));
            }

            if (video != null)
            {
                if (!Extensions.IsVideoFile(video.FileName))
                    throw new ArgumentException("Not a video file provided for video.");
                if (newEpisode.VideoPath == null)
                    throw new ArgumentException("No path provided for video.");

                newEpisode.VideoPath = SanitizeFileName(Path.Combine(_configuration["Paths:Videos"]!, series.Title, newEpisode.VideoPath));
                int bufferSize = int.TryParse(_configuration["VideoBufferSize"], out var size) ? size : 10000;
                await _FileProcessingService.ProcessVideoFileAsync(video, Path.Combine(_configuration["Paths:Root"]!, newEpisode.VideoPath), this, bufferSize);
            }

            series.UpdateDateModified();
            await _MediaRepository.UpdateSeries(series);

            newEpisode = await _MediaRepository.CreateEpisodeAsync(newEpisode);
            return _mapper.Map<EpisodeDto>(newEpisode);
        }

        /// <summary>
        /// Add an episode using pre-streamed files (video already written to disk).
        /// </summary>
        public async Task<EpisodeDto> AddEpisodeStreamed(EpisodeDto episodeDto, string? posterTempPath, string? videoFinalPath)
        {
            var newEpisode = _mapper.Map<Episode>(episodeDto);
            newEpisode.Initialize();

            var series = await _MediaRepository.GetSeriesByIdAsync(newEpisode.SeriesId);
            if (series == null)
                throw new ArgumentException("The episode is added to a non-existent series");

            if (newEpisode.EpisodeNumber == null)
                newEpisode.EpisodeNumber = await _MediaRepository.GetLastEpisodeOfASeasonAsync(newEpisode.SeriesId, newEpisode.SeasonNumber) + 1;

            if (!string.IsNullOrEmpty(posterTempPath) && File.Exists(posterTempPath))
            {
                if (newEpisode.PosterPath == null)
                    throw new ArgumentException("No path provided for poster.");

                var finalPosterPath = Path.Combine(
                    _configuration["Paths:Root"]!,
                    _configuration["Paths:Images"]!,
                    series.Title,
                    SanitizeFileName(newEpisode.PosterPath));

                var posterDir = Path.GetDirectoryName(finalPosterPath);
                if (!string.IsNullOrEmpty(posterDir) && !Directory.Exists(posterDir))
                    Directory.CreateDirectory(posterDir);

                File.Move(posterTempPath, finalPosterPath, overwrite: true);
                newEpisode.PosterPath = SanitizeFileName(Path.Combine(_configuration["Paths:Images"]!, series.Title, newEpisode.PosterPath));
            }

            if (!string.IsNullOrEmpty(videoFinalPath) && File.Exists(videoFinalPath))
            {
                // Video was already streamed to final location, just update the path
                var relativePath = Path.GetRelativePath(_configuration["Paths:Root"]!, videoFinalPath);
                newEpisode.VideoPath = relativePath.Replace("\\", "/");

                // Normalize video in background
                _ = Task.Run(() => NormalizeVideo(videoFinalPath));
            }

            series.UpdateDateModified();
            await _MediaRepository.UpdateSeries(series);

            newEpisode = await _MediaRepository.CreateEpisodeAsync(newEpisode);
            return _mapper.Map<EpisodeDto>(newEpisode);
        }

        public async Task<ICollection<Episode>> AddManyEpisodesToASeries(Guid seriesId, int seasonNumber, int episodesNumber, string? posterPath)
        {
            var lastEpisodeNumber = await _MediaRepository.GetLastEpisodeOfASeasonAsync(seriesId, seasonNumber);
            var newEpisodes = new List<Episode>();

            for (var episodeNumber = 1; episodeNumber <= episodesNumber; episodeNumber++)
            {
                var newEpisode = new Episode();
                newEpisode.SeriesId = seriesId;
                newEpisode.SeasonNumber = seasonNumber;
                newEpisode.EpisodeNumber = lastEpisodeNumber + episodeNumber;
                newEpisode.PosterPath = posterPath;

                newEpisode.Initialize();

                newEpisode = await _MediaRepository.CreateEpisodeAsync(newEpisode);

                newEpisodes.Add(newEpisode);
            }

            return newEpisodes;
        }

        // editing
        public async Task<MovieDto?> EditMovie(Guid id, MovieDto editedMovieDto, IFormFile? poster, IFormFile? video)
        {
            var movieToEdit = await _MediaRepository.GetMovieByIdAsync(id);
            if (movieToEdit == null)
                return null;

            // Map DTO to entity first
            _mapper.Map(editedMovieDto, movieToEdit);

            // Then handle file uploads (these override the mapped paths)
            if (poster != null)
            {
                if (!Extensions.IsImageFile(poster.FileName))
                    throw new ArgumentException("Not an image file provided for poster.");

                var newPosterPath = editedMovieDto.PosterPath;
                if (string.IsNullOrWhiteSpace(newPosterPath))
                    throw new ArgumentException("No path provided for poster.");

                movieToEdit.PosterPath = _configuration["Paths:Images"] + SanitizeFileName(newPosterPath);
                await _FileProcessingService.ProcessFileAsync(poster, Path.Combine(_configuration["Paths:Root"]!, movieToEdit.PosterPath));
            }

            if (video != null)
            {
                if (!Extensions.IsVideoFile(video.FileName))
                    throw new ArgumentException("Not a video file provided for video.");

                var newVideoPath = editedMovieDto.VideoPath;
                if (string.IsNullOrWhiteSpace(newVideoPath))
                    throw new ArgumentException("No path provided for video.");

                movieToEdit.VideoPath = _configuration["Paths:Videos"] + SanitizeFileName(newVideoPath);
                int bufferSize = int.TryParse(_configuration["VideoBufferSize"], out var size) ? size : 10000;
                await _FileProcessingService.ProcessVideoFileAsync(video, Path.Combine(_configuration["Paths:Root"]!, movieToEdit.VideoPath), this, bufferSize);
            }

            movieToEdit.UpdateDateModified();
            await _MediaRepository.UpdateMovie(movieToEdit);

            return _mapper.Map<MovieDto>(movieToEdit);
        }

        public async Task<SeriesDto?> EditSeries(Guid id, SeriesDto editedSeriesDto, IFormFile? poster)
        {
            var seriesToEdit = await _MediaRepository.GetSeriesByIdAsync(id);
            if (seriesToEdit == null)
                return null;

            // Map DTO to entity first
            _mapper.Map(editedSeriesDto, seriesToEdit);

            // Then handle file uploads (these override the mapped paths)
            if (poster != null)
            {
                if (!Extensions.IsImageFile(poster.FileName))
                    throw new ArgumentException("Not an image file provided for poster.");

                var newPosterPath = editedSeriesDto.PosterPath;
                if (string.IsNullOrWhiteSpace(newPosterPath))
                    throw new ArgumentException("No path provided for poster.");

                seriesToEdit.PosterPath = SanitizeFileName(Path.Combine(_configuration["Paths:Images"]!, seriesToEdit.Title, newPosterPath));
                await _FileProcessingService.ProcessFileAsync(poster, Path.Combine(_configuration["Paths:Root"]!, seriesToEdit.PosterPath));
            }

            seriesToEdit.UpdateDateModified();
            await _MediaRepository.UpdateSeries(seriesToEdit);

            return _mapper.Map<SeriesDto>(seriesToEdit);
        }

        public async Task<EpisodeDto?> EditEpisode(Guid id, EpisodeDto editedEpisodeDto, IFormFile? poster, IFormFile? video)
        {
            var episodeToEdit = await _MediaRepository.GetEpisodeByIdAsync(id);
            if (episodeToEdit == null)
                return null;

            // Store series reference before mapping (in case it gets cleared)
            var seriesTitle = episodeToEdit.Series?.Title;

            // Map DTO to entity first
            _mapper.Map(editedEpisodeDto, episodeToEdit);

            // Then handle file uploads (these override the mapped paths)
            if (poster != null)
            {
                if (!Extensions.IsImageFile(poster.FileName))
                    throw new ArgumentException("Not an image file provided for poster.");

                var newPosterPath = editedEpisodeDto.PosterPath;
                if (string.IsNullOrWhiteSpace(newPosterPath))
                    throw new ArgumentException("No path provided for poster.");

                episodeToEdit.PosterPath = SanitizeFileName(Path.Combine(_configuration["Paths:Images"]!, seriesTitle!, newPosterPath));
                await _FileProcessingService.ProcessFileAsync(poster, Path.Combine(_configuration["Paths:Root"]!, episodeToEdit.PosterPath));
            }

            if (video != null)
            {
                if (!Extensions.IsVideoFile(video.FileName))
                    throw new ArgumentException("Not a video file provided for video.");

                var newVideoPath = editedEpisodeDto.VideoPath;
                if (string.IsNullOrWhiteSpace(newVideoPath))
                    throw new ArgumentException("No path provided for video.");

                episodeToEdit.VideoPath = SanitizeFileName(Path.Combine(_configuration["Paths:Videos"]!, seriesTitle!, newVideoPath));
                int bufferSize = int.TryParse(_configuration["VideoBufferSize"], out var size) ? size : 10000;
                await _FileProcessingService.ProcessVideoFileAsync(video, Path.Combine(_configuration["Paths:Root"]!, episodeToEdit.VideoPath), this, bufferSize);
            }

            episodeToEdit.UpdateDateModified();
            await _MediaRepository.UpdateEpisode(episodeToEdit);

            return _mapper.Map<EpisodeDto>(episodeToEdit);
        }

        // removing

        public async Task<bool> DeleteMedia(Guid id)
        {
            var mediaToDelete = await _MediaRepository.GetByIdAsync(id);

            if (mediaToDelete == null)
            {
                return false;
            }

            // delete poster and video files from storage
            if (mediaToDelete.PosterPath != null && System.IO.File.Exists(Path.Combine(_configuration["Paths:Root"]!, mediaToDelete.PosterPath)))
            {
                System.IO.File.Delete(Path.Combine(_configuration["Paths:Root"]!, mediaToDelete.PosterPath));
            }

            if (mediaToDelete.VideoPath != null && System.IO.File.Exists(Path.Combine(_configuration["Paths:Root"]!, mediaToDelete.VideoPath)))
            {
                System.IO.File.Delete(Path.Combine(_configuration["Paths:Root"]!, mediaToDelete.VideoPath));
            }

            await _ContinueWatchingService.DeleteForAllUsersByMediaIdAsync(id);

            await _MediaRepository.DeleteAsync(mediaToDelete);

            return true;
        }

        // getting by id

        public async Task<Movie?> GetMovieById(Guid id)
        {
            return await _MediaRepository.GetMovieByIdAsync(id);
        }

        public async Task<Series?> GetSeriesById(Guid id)
        {
            return await _MediaRepository.GetSeriesByIdAsync(id);
        }

        public async Task<Episode?> GetEpisodeById(Guid id)
        {
            return await _MediaRepository.GetEpisodeByIdAsync(id);
        }

        public async Task NormalizeVideo(string videoPath)
        {
            string scriptPath;
            string shell;

            videoPath = Path.Combine(Directory.GetCurrentDirectory(), videoPath);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Windows
                scriptPath = Path.Combine(Directory.GetCurrentDirectory(), _configuration["Paths:ShellScripts"] + "convert_to_mp4_aac.bat");
                shell = "cmd.exe";
            }
            else
            {
                // Linux
                scriptPath = Path.Combine(Directory.GetCurrentDirectory(), _configuration["Paths:ShellScripts"] + "convert_to_mp4_aac.sh"); ;
                shell = "/bin/bash";
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            ? $"/c \"{scriptPath}\" \"{videoPath}\""
                            : $"-c \"{scriptPath}\" \"{videoPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(processStartInfo)!)
                {
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorsTask = process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    var output = await outputTask;
                    var errors = await errorsTask;

                    _logger.LogInformation("Output:");
                    _logger.LogInformation(output);
                    _logger.LogError("Errors:");
                    _logger.LogError(errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
            }
        }

        public async Task<MediaAttribute> AddAttribute(MediaAttribute mediaAttribute)
        {
            mediaAttribute.Initialize();
            return await _MediaAttributeRepository.AddAsync(mediaAttribute);
        }

        public async Task<MediaAttribute?> UpdateAttribute(MediaAttribute mediaAttribute, IFormFile? file)
        {
            var attributeToUpdate = await _MediaAttributeRepository.GetByIdAsync(mediaAttribute.MediaId);
            if (attributeToUpdate == null)
                return null;

            if (file != null)
            {
                var media = await _MediaRepository.GetByIdAsync(mediaAttribute.MediaId);
                if (media?.VideoPath == null)
                    throw new ArgumentException("Media must have a video for adding an attribute");

                var extension = Path.GetExtension(file.FileName);
                if (extension == null)
                    throw new ArgumentException("File does not have an extension");

                if (extension == ".srt")
                    extension = ".vtt";

                var newAttributePath = Path.ChangeExtension(media.VideoPath, null) + "_" + mediaAttribute.Type + "_" + mediaAttribute.Language + extension;

                // Delete old attribute file if it exists
                if (attributeToUpdate.AttributePath != null && File.Exists(Path.Combine(_configuration["Paths:Videos"]!, attributeToUpdate.AttributePath)))
                {
                    File.Delete(Path.Combine(_configuration["Paths:Videos"]!, attributeToUpdate.AttributePath));
                }

                await _FileProcessingService.ProcessFileAsync(file, Path.Combine(_configuration["Paths:Videos"]!, newAttributePath));
                mediaAttribute.AttributePath = newAttributePath;
            }

            _mapper.Map(mediaAttribute, attributeToUpdate);
            await _MediaAttributeRepository.UpdateAsync(attributeToUpdate);

            return attributeToUpdate;
        }

        public async Task<bool> DeleteAttribute(Guid mediaId, string attributeType, string language)
        {
            // Verify media exists first
            var media = await _MediaRepository.GetByIdAsync(mediaId);
            if (media == null)
                return false;

            // Query all attributes for this media
            var allAttributes = await _MediaAttributeRepository.GetAllAsync();
            var attribute = allAttributes.FirstOrDefault(a =>
                a.MediaId == mediaId && a.Type == attributeType && a.Language == language);

            if (attribute == null)
                return false;

            // Delete attribute file from storage
            if (attribute.AttributePath != null && File.Exists(Path.Combine(_configuration["Paths:Videos"]!, attribute.AttributePath)))
            {
                File.Delete(Path.Combine(_configuration["Paths:Videos"]!, attribute.AttributePath));
            }

            await _MediaAttributeRepository.DeleteAsync(attribute);
            return true;
        }

        public string SanitizeFileName(string fileName)
        {
            fileName = fileName.Replace("&", "and").Replace(":", "").Replace("?", "");

            return fileName;
        }

        public async Task<VideoStreamInfoDto> GetVideoStreamInfoAsync(Guid mediaId)
        {
            var media = await GetById(mediaId);

            if (media == null)
            {
                return new VideoStreamInfoDto { ErrorMessage = "Media not found" };
            }

            if (string.IsNullOrEmpty(media.VideoPath))
            {
                return new VideoStreamInfoDto { ErrorMessage = "No video file associated with this media" };
            }

            var rootPath = _configuration["Paths:Root"];
            if (string.IsNullOrEmpty(rootPath))
            {
                return new VideoStreamInfoDto { ErrorMessage = "Server configuration error" };
            }

            var relativePath = media.VideoPath;
            if (relativePath.StartsWith("/mymdb/static/"))
            {
                relativePath = relativePath.Substring("/mymdb/static/".Length);
            }

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), rootPath, relativePath);

            if (!File.Exists(fullPath))
            {
                return new VideoStreamInfoDto { ErrorMessage = "Video file not found on server" };
            }

            // Determine content type based on extension
            var extension = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mkv" => "video/x-matroska",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                _ => "application/octet-stream"
            };

            return new VideoStreamInfoDto
            {
                FullPath = fullPath,
                ContentType = contentType
            };
        }
    }
}

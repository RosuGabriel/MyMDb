using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyMDb.DTOs;
using MyMDb.Models;
using MyMDb.ServiceInterfaces;
using MyMDb.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MyMDb.Helpers;

namespace MyMDb.Controllers
{
    [ApiController]
    [Route("mymdb/api/media")]
    public class MediaController : Controller
    {
        private readonly IMediaService _mediaService;
        private readonly IContinueWatchingService _continueWatchingService;
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly int bufferSize;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MediaController> _logger;
        private readonly IWebHostEnvironment _env;

        public MediaController(IMediaService mediaService, IContinueWatchingService continueWatchingService, IFileProcessingService fileProcessingService, IMapper mapper, IConfiguration configuration, ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<MediaController> logger, IWebHostEnvironment env)
        {
            _mediaService = mediaService;
            _continueWatchingService = continueWatchingService;
            _fileProcessingService = fileProcessingService;
            _mapper = mapper;
            _configuration = configuration;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _env = env;
            if (_configuration["VideoBufferSize"] != null)
            {
                bufferSize = int.Parse(_configuration["VideoBufferSize"]!);
            }
            else
            {
                bufferSize = 10000;
            }
        }

        // -------------------- get all

        [HttpGet]
        [Authorize]
        [Route("movies_and_series")]
        public async Task<IActionResult> GetMoviesAndSeries()
        {
            var allMedia = await _mediaService.GetAllMedia();
            var mediaDtos = _mapper.Map<List<MediaDto>>(allMedia);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(mediaDtos);
        }

        [HttpGet]
        [Authorize]
        [Route("movies")]
        public async Task<IActionResult> GetMovies()
        {
            var allMovies = await _mediaService.GetAllMovies();
            var moviesDtos = _mapper.Map<List<MovieDto>>(allMovies);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(moviesDtos);
        }

        [HttpGet]
        [Authorize]
        [Route("series")]
        public async Task<IActionResult> GetSeries()
        {
            var allSeries = await _mediaService.GetAllSeries();
            var seriesDtos = _mapper.Map<List<SeriesDto>>(allSeries);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(seriesDtos);
        }

        // -------------------- get by id

        [HttpGet]
        [Authorize]
        [Route("{id}")]
        public async Task<IActionResult> GetMedia(Guid id)
        {
            var media = await _mediaService.GetById(id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (media == null)
            {
                return NotFound();
            }

            if (media.MediaType == "Movie")
            {
                var movie = await _mediaService.GetMovieById(id);
                return Ok(movie);
            }
            else if (media.MediaType == "Series")
            {
                var series = await _mediaService.GetSeriesById(id);
                return Ok(series);
            }
            else if (media.MediaType == "Episode")
            {
                var episode = await _mediaService.GetEpisodeById(id);
                return Ok(episode);
            }

            return NotFound();
        }

        [HttpGet]
        [Authorize]
        [Route("series/{seriesId}/episodes")]
        public async Task<IActionResult> GetEpisodesOfASeries(Guid seriesId)
        {
            var episodes = await _mediaService.GetEpisodesOfASeries(seriesId);
            var episodeDtos = _mapper.Map<List<EpisodeDto>>(episodes);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (episodeDtos.Count == 0)
            {
                return NotFound();
            }

            return Ok(episodeDtos);
        }

        // -------------------- video streaming

        [HttpPost]
        [Authorize]
        [Route("stream-token/{id}")]
        public async Task<IActionResult> GetStreamToken(Guid id)
        {
            // Verify media exists and has video
            var media = await _mediaService.GetById(id);
            
            if (media == null)
            {
                return NotFound("Media not found");
            }

            if (string.IsNullOrEmpty(media.VideoPath))
            {
                return NotFound("No video file associated with this media");
            }

            // Get the current JWT token from the authenticated user
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("No token provided");
            }

            // Set HTTP-only cookie with the JWT for streaming
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(), // Only require HTTPS in production
                SameSite = SameSiteMode.Lax, // Lax allows cookie in video element requests
                Path = "/mymdb/api/media/stream",
                MaxAge = TimeSpan.FromHours(4) // Cookie valid for 4 hours
            };

            Response.Cookies.Append("stream_auth", token, cookieOptions);

            return Ok(new { success = true });
        }

        [HttpGet]
        [Authorize]
        [Route("stream/{id}")]
        public async Task<IActionResult> StreamVideo(Guid id)
        {
            var streamInfo = await _mediaService.GetVideoStreamInfoAsync(id);

            if (!streamInfo.IsSuccess)
            {
                if (streamInfo.ErrorMessage == "Server configuration error")
                {
                    _logger.LogError("Paths:Root is not configured");
                    return StatusCode(500, streamInfo.ErrorMessage);
                }

                if (streamInfo.ErrorMessage == "Video file not found on server")
                {
                    _logger.LogWarning("Video file not found for media {MediaId}", id);
                }

                return NotFound(streamInfo.ErrorMessage);
            }

            return PhysicalFile(streamInfo.FullPath, streamInfo.ContentType, enableRangeProcessing: true);
        }

        // -------------------- add

        [HttpPost]
        [Authorize("admin")]
        [Route("add_attribute")]
        public async Task<IActionResult> AddAttribute([FromForm] MediaAttributeDto attributeDto, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var media = await _mediaService.GetById(attributeDto.MediaId);

            if (media == null)
            {
                return NotFound("Media for attribute not found");
            }

            if (file == null)
            {
                return BadRequest("Attribute file not existent");
            }

            var extension = Path.GetExtension(file.FileName);
            if (extension == null)
            {
                return BadRequest("File does not have an extension");
            }
            // Saved as vtt but still needs conversion, this is for not editing the path again after the conversion
            if (extension == ".srt")
            {
                extension = ".vtt";
            }

            if (media.VideoPath != null)
            {
                attributeDto.AttributePath = Path.ChangeExtension(media.VideoPath, null) + "_" + attributeDto.Type + "_" + attributeDto.Language + extension;
            }
            else
            {
                return BadRequest("Media must have a video for adding an attribute");
            }

            await _fileProcessingService.ProcessFileAsync(file, _configuration["Paths:Root"] + attributeDto.AttributePath);

            var attribute = _mapper.Map<MediaAttribute>(attributeDto);

            var newAttribute = await _mediaService.AddAttribute(attribute);

            return Ok(newAttribute);
        }

        [HttpPost]
        [Authorize("admin")]
        [Route("add_movie")]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(10L * 1024 * 1024 * 1024)] // 10GB limit
        public async Task<IActionResult> AddMovie()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest("Expected a multipart request.");
            }

            try
            {
                _logger.LogInformation("Starting streamed movie upload");

                var tempPath = Path.Combine(_configuration["Paths:Root"]!, "temp");
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                var formData = await StreamingHelpers.StreamMultipartToDiskAsync(
                    Request,
                    videosBasePath: Path.Combine(_configuration["Paths:Root"]!, _configuration["Paths:Videos"]!),
                    imagesBasePath: Path.Combine(_configuration["Paths:Root"]!, _configuration["Paths:Images"]!),
                    tempPath: tempPath,
                    bufferSize: bufferSize,
                    sanitizeFileName: _mediaService.SanitizeFileName,
                    _logger,
                    HttpContext.RequestAborted);

                // Build MovieDto from form fields
                var movieDto = new MovieDto
                {
                    Title = formData.FormFields.GetValueOrDefault("title"),
                    Description = formData.FormFields.GetValueOrDefault("description"),
                    PosterPath = formData.FormFields.GetValueOrDefault("posterPath"),
                    VideoPath = formData.FormFields.GetValueOrDefault("videoPath")
                };

                if (formData.FormFields.TryGetValue("releaseDate", out var releaseDateStr) &&
                    DateTime.TryParse(releaseDateStr, out var releaseDate))
                {
                    movieDto.ReleaseDate = releaseDate;
                }

                string? posterTempPath = formData.Files.GetValueOrDefault("poster")?.TempFilePath;
                string? videoFinalPath = formData.Files.GetValueOrDefault("video")?.TempFilePath;

                _logger.LogInformation("Streamed upload complete. Poster: {Poster}, Video: {Video}",
                    posterTempPath ?? "none", videoFinalPath ?? "none");

                var newMovie = await _mediaService.AddMovieStreamed(movieDto, posterTempPath, videoFinalPath);
                return Ok(newMovie);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument during movie upload");
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Movie upload was cancelled");
                return StatusCode(499, "Upload cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during streamed movie upload");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPost]
        [Authorize("admin")]
        [Route("add_series")]
        public async Task<IActionResult> AddSeries([FromForm] SeriesDto series, IFormFile? poster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newSeries = await _mediaService.AddSeries(series, poster);
                return Ok(newSeries);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize("admin")]
        [Route("add_episode")]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(10L * 1024 * 1024 * 1024)] // 10GB limit
        public async Task<IActionResult> AddEpisode()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest("Expected a multipart request.");
            }

            try
            {
                _logger.LogInformation("Starting streamed episode upload");

                var tempPath = Path.Combine(_configuration["Paths:Root"]!, "temp");
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                // We need seriesId first to determine the correct video path
                // For episodes, we'll stream to temp first and move after
                var formData = await StreamingHelpers.StreamMultipartToDiskAsync(
                    Request,
                    videosBasePath: tempPath, // Temporarily stream to temp, will be moved by service
                    imagesBasePath: Path.Combine(_configuration["Paths:Root"]!, _configuration["Paths:Images"]!),
                    tempPath: tempPath,
                    bufferSize: bufferSize,
                    sanitizeFileName: _mediaService.SanitizeFileName,
                    _logger,
                    HttpContext.RequestAborted);

                // Build EpisodeDto from form fields
                var episodeDto = new EpisodeDto
                {
                    Title = formData.FormFields.GetValueOrDefault("title"),
                    Description = formData.FormFields.GetValueOrDefault("description"),
                    PosterPath = formData.FormFields.GetValueOrDefault("posterPath"),
                    VideoPath = formData.FormFields.GetValueOrDefault("videoPath")
                };

                if (formData.FormFields.TryGetValue("seriesId", out var seriesIdStr) &&
                    Guid.TryParse(seriesIdStr, out var seriesId))
                {
                    episodeDto.SeriesId = seriesId;
                }

                if (formData.FormFields.TryGetValue("seasonNumber", out var seasonStr) &&
                    int.TryParse(seasonStr, out var seasonNumber))
                {
                    episodeDto.SeasonNumber = seasonNumber;
                }

                if (formData.FormFields.TryGetValue("episodeNumber", out var episodeStr) &&
                    int.TryParse(episodeStr, out var episodeNumber))
                {
                    episodeDto.EpisodeNumber = episodeNumber;
                }

                if (formData.FormFields.TryGetValue("releaseDate", out var releaseDateStr) &&
                    DateTime.TryParse(releaseDateStr, out var releaseDate))
                {
                    episodeDto.ReleaseDate = releaseDate;
                }

                string? posterTempPath = formData.Files.GetValueOrDefault("poster")?.TempFilePath;
                string? videoTempPath = formData.Files.GetValueOrDefault("video")?.TempFilePath;

                // For episodes, we need to move the video to the correct series folder
                string? videoFinalPath = null;
                if (!string.IsNullOrEmpty(videoTempPath) && System.IO.File.Exists(videoTempPath) && episodeDto.SeriesId.HasValue)
                {
                    var series = await _mediaService.GetSeriesById(episodeDto.SeriesId.Value);
                    if (series != null && !string.IsNullOrEmpty(episodeDto.VideoPath))
                    {
                        videoFinalPath = Path.Combine(
                            _configuration["Paths:Root"]!,
                            _configuration["Paths:Videos"]!,
                            series.Title,
                            _mediaService.SanitizeFileName(episodeDto.VideoPath));

                        var videoDir = Path.GetDirectoryName(videoFinalPath);
                        if (!string.IsNullOrEmpty(videoDir) && !Directory.Exists(videoDir))
                            Directory.CreateDirectory(videoDir);

                        System.IO.File.Move(videoTempPath, videoFinalPath, overwrite: true);
                    }
                }

                _logger.LogInformation("Streamed episode upload complete. Poster: {Poster}, Video: {Video}",
                    posterTempPath ?? "none", videoFinalPath ?? "none");

                var newEpisode = await _mediaService.AddEpisodeStreamed(episodeDto, posterTempPath, videoFinalPath);
                return Ok(newEpisode);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument during episode upload");
                return BadRequest(ex.Message);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Episode upload was cancelled");
                return StatusCode(499, "Upload cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during streamed episode upload");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize("admin")]
        [Route("add_many_episodes")]
        public async Task<IActionResult> AddManyEpisodes(Guid seriesId, int seasonNumber, int episodesNumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var series = await _mediaService.GetSeriesById(seriesId);

            if (series == null)
            {
                return NotFound("Series not found");
            }

            var newEpisodes = await _mediaService.AddManyEpisodesToASeries(seriesId, seasonNumber, episodesNumber, series.PosterPath);

            return Ok(newEpisodes);
        }

        // -------------------- edit

        [HttpPost]
        [Authorize("admin")]
        [Route("edit_movie/{id}")]
        public async Task<IActionResult> EditMovie(Guid id, [FromForm] MovieDto movieToEdit, IFormFile? poster, IFormFile? video)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != movieToEdit.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var updatedMovie = await _mediaService.EditMovie(id, movieToEdit, poster, video);
                if (updatedMovie == null)
                    return NotFound();

                return Ok(updatedMovie);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize("admin")]
        [Route("edit_series/{id}")]
        public async Task<IActionResult> EditSeries(Guid id, [FromForm] SeriesDto seriesToEdit, IFormFile? poster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != seriesToEdit.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var updatedSeries = await _mediaService.EditSeries(id, seriesToEdit, poster);
                if (updatedSeries == null)
                    return NotFound();

                return Ok(updatedSeries);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize("admin")]
        [Route("edit_episode/{id}")]
        public async Task<IActionResult> EditEpisode(Guid id, [FromForm] EpisodeDto episodeToEdit, IFormFile? poster, IFormFile? video)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != episodeToEdit.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var updatedEpisode = await _mediaService.EditEpisode(id, episodeToEdit, poster, video);
                if (updatedEpisode == null)
                    return NotFound();

                return Ok(updatedEpisode);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // -------------------- edit/update attribute

        [HttpPut]
        [Authorize("admin")]
        [Route("update_attribute")]
        public async Task<IActionResult> UpdateAttribute([FromForm] MediaAttributeDto attributeDto, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var attribute = _mapper.Map<MediaAttribute>(attributeDto);
                var updatedAttribute = await _mediaService.UpdateAttribute(attribute, file);

                if (updatedAttribute == null)
                    return NotFound("Attribute not found");

                return Ok(updatedAttribute);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // -------------------- delete

        [HttpDelete]
        [Authorize("admin")]
        [Route("delete_attribute/{mediaId}")]
        public async Task<IActionResult> DeleteAttribute(Guid mediaId, [FromQuery] string attributeType, [FromQuery] string language)
        {
            if (string.IsNullOrWhiteSpace(attributeType) || string.IsNullOrWhiteSpace(language))
            {
                return BadRequest("Attribute type and language are required");
            }

            try
            {
                var deleted = await _mediaService.DeleteAttribute(mediaId, attributeType, language);

                if (!deleted)
                    return NotFound("Attribute not found");

                return Ok(new { message = "Attribute deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete]
        [Authorize("admin")]
        [Route("delete_media/{id}")]
        public async Task<IActionResult> DeleteMedia(Guid id)
        {
            var deleted = await _mediaService.DeleteMedia(id);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!deleted)
            {
                return NotFound();
            }

            return Ok();
        }

        // -------------------- continue watching

        [HttpGet]
        [Authorize]
        [Route("continue_watching/{mediaId}/{episodeId?}")]
        public async Task<IActionResult> GetContinueWatchingById(Guid mediaId, Guid? episodeId = null)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return NotFound();
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest("User not provided");
            }

            try
            {
                var continueWatchings = await _continueWatchingService.GetByUserIdAndMediaIdAsync(userId, mediaId, episodeId);

                if (continueWatchings == null)
                {
                    return NoContent(); // 204 - no watch history exists yet
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);

                }

                return Ok(continueWatchings);
            }
            catch (ActionResponseExceptions.BaseException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred: " + ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("continue_watching")]
        public async Task<IActionResult> GetContinueWatching()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return NotFound();
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest("User not provided");
            }

            try
            {
                var continueWatchings = await _continueWatchingService.GetAllByUserIdAsync(userId);

                if (continueWatchings == null)
                {
                    return NotFound("Continue watching not found");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);

                }

                return Ok(continueWatchings);
            }
            catch (ActionResponseExceptions.BaseException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred: " + ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("continue_watching")]
        public async Task<IActionResult> AddOrUpdateContinueWatching([FromBody] ContinueWatching updatedContinueWatching)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return NotFound();
            }

            if (updatedContinueWatching.MediaId == null)
            {
                return BadRequest("ContinueWatching not provided");
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest("User not provided");
            }

            try
            {
                var continueWatching = await _continueWatchingService.AddOrUpdateAsync(userId, updatedContinueWatching.MediaId, updatedContinueWatching.EpisodeId, updatedContinueWatching.WatchedTime, updatedContinueWatching.Duration);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(continueWatching);
            }
            catch (ActionResponseExceptions.BaseException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred: " + ex.Message);
            }
        }

        [HttpDelete]
        [Authorize]
        [Route("continue_watching")]
        public async Task<IActionResult> DeleteContinueWatching([FromBody] ContinueWatching continueWatching)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return NotFound();
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest("User not provided");
            }

            try
            {
                await _continueWatchingService.DeleteAsync(userId, continueWatching.MediaId, continueWatching.EpisodeId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok();
            }
            catch (ActionResponseExceptions.BaseException ex)
            {
                return StatusCode(ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred: " + ex.Message);
            }
        }
    }
}

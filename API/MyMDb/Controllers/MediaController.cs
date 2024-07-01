using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyMDb.DTOs;
using MyMDb.Models;
using MyMDb.ServiceInterfaces;
using MyMDb.Services;
using MyMDb.Data;

namespace MyMDb.Controllers
{
    [ApiController]
    [Route("api/media")]
    public class MediaController : Controller 
    {
        private readonly IMediaService _mediaService;
        private readonly IMapper _mapper;

        public MediaController(IMediaService mediaService, IMapper mapper) 
        {
            _mediaService = mediaService;
            _mapper = mapper;
        }

        // -------------------- get all

        [HttpGet]
        [Route("movies&series")]
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

            return Ok(media);
        }

        [HttpGet]
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

        // -------------------- add

        [HttpPost]
        [Route("add_movie")]
        public async Task<IActionResult> AddMovie(string? title, string? description, DateTime? releaseDate)
        {
            var posterPath = "";
            var videoPath = "";
            var newMovie = await _mediaService.AddMovie(title, description, releaseDate, posterPath, videoPath);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newMovie);
        }

        [HttpPost]
        [Route("add_series")]
        public async Task<IActionResult> AddSeries(string? title, string? description, DateTime? releaseDate)
        {
            var posterPath = "";
            var newSeries = await _mediaService.AddSeries(title, description, releaseDate, posterPath);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newSeries);
        }

        [HttpPost]
        [Route("add_episode")]
        public async Task<IActionResult> AddEpisode(string? title, string? description, DateTime? releaseDate, int seasonNumber, Guid seriesId, int? episodeNumber)
        {
            var posterPath = "";
            var videoPath = "";
            var newEpisode = await _mediaService.AddEpisode(title, description, releaseDate, seasonNumber,  seriesId, episodeNumber, posterPath, videoPath);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newEpisode);
        }

        [HttpPost]
        [Route("add_many_episodes")]
        public async Task<IActionResult> AddManyEpisodes(Guid seriesId, int seasonNumber, int episodesNumber)
        {
            var newEpisodes = await _mediaService.AddManyEpisodesToASeries(seriesId, seasonNumber, episodesNumber);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newEpisodes);
        }

        // -------------------- file upload

        [HttpPost]
        [DisableRequestSizeLimit] // allows big uploads
        [Route("{id}/upload_movie_content")]
        public async Task<IActionResult> UploadMoviePoster(Guid id, IFormFile? poster, IFormFile? video)
        {
            if (poster == null && video == null)
            {
                return BadRequest("No file uploaded.");
            }

            var movie = await _mediaService.GetMovieById(id);
            if (movie == null)
            {
                return NotFound();
            }

            if (poster != null)
            {
                var posterPath = Path.Combine(DefaultValues.IMAGES_PATH, movie.Id.ToString()) +
                                 Path.GetExtension(poster.FileName);
                using (var stream = new FileStream(posterPath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }
                movie.PosterPath = posterPath;
            }

            if (video != null)
            {
                var videoPath = Path.Combine(DefaultValues.VIDEOS_PATH, movie.Id.ToString()) +
                                Path.GetExtension(video.FileName);
                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    await video.CopyToAsync(stream);
                }
                movie.VideoPath = videoPath;
            }

            await _mediaService.EditMovie(id, movie);

            return Ok(movie);
        }

        [HttpPost]
        [Route("{id}/upload_series_poster")]
        public async Task<IActionResult> UploadSeriesPoster(Guid id, IFormFile? poster)
        {
            if (poster == null || poster.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var series = await _mediaService.GetSeriesById(id);
            if (series == null)
            {
                return NotFound();
            }

            var posterPath = Path.Combine(DefaultValues.IMAGES_PATH, series.Id.ToString()) +
                             Path.GetExtension(poster.FileName);

            using (var stream = new FileStream(posterPath, FileMode.Create))
            {
                await poster.CopyToAsync(stream);
            }

            series.PosterPath = posterPath;

            await _mediaService.EditSeries(id, series);

            return Ok(series);
        }

        [HttpPost]
        [Route("{id}/upload_episode_content")]
        public async Task<IActionResult> UploadEpisodePoster(Guid id, IFormFile? poster, IFormFile? video)
        {
            if (poster == null && video == null)
            {
                return BadRequest("No file uploaded.");
            }

            var episode = await _mediaService.GetMovieById(id);
            if (episode == null)
            {
                return NotFound();
            }

            if (poster != null)
            {
                var posterPath = Path.Combine(DefaultValues.IMAGES_PATH, episode.Id.ToString()) +
                                 Path.GetExtension(poster.FileName);
                using (var stream = new FileStream(posterPath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }
                episode.PosterPath = posterPath;
            }

            if (video != null)
            {
                var videoPath = Path.Combine(DefaultValues.VIDEOS_PATH, episode.Id.ToString()) +
                                Path.GetExtension(video.FileName);
                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    await video.CopyToAsync(stream);
                }
                episode.VideoPath = videoPath;
            }


            await _mediaService.EditMovie(id, episode);

            return Ok(episode);
        }

        // -------------------- edit

        [HttpPost]
        [Route("edit_movie/{id}")]
        public async Task<IActionResult> EditMovie(Guid id, [FromBody] MovieDto movieToEdit)
        {
            movieToEdit.Id = id;

            if (id != movieToEdit.Id)
            {
                return BadRequest();
            }

            var currentMovie = await _mediaService.GetMovieById(id);
            if(currentMovie == null)
            {
                return NotFound();
            }

            _mapper.Map(movieToEdit, currentMovie);

            await _mediaService.EditMovie(id, currentMovie);

            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            return Ok(currentMovie);
        }

        [HttpPost]
        [Route("edit_series/{id}")]
        public async Task<IActionResult> EditSeries(Guid id, [FromBody] SeriesDto seriesToEdit)
        {
            seriesToEdit.Id = id;

            if (id != seriesToEdit.Id)
            {
                return BadRequest();
            }

            var currentSeries = await _mediaService.GetMovieById(id);
            if (currentSeries == null)
            {
                return NotFound();
            }

            _mapper.Map(seriesToEdit, currentSeries);

            await _mediaService.EditMovie(id, currentSeries);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(currentSeries);
        }

        [HttpPost]
        [Route("edit_episode/{id}")]
        public async Task<IActionResult> EditEpisode(Guid id, [FromBody] EpisodeDto episodeToEdit)
        {
            episodeToEdit.Id = id;

            if (id != episodeToEdit.Id)
            {
                return BadRequest();
            }

            var currentEpisode = await _mediaService.GetMovieById(id);
            if (currentEpisode == null)
            {
                return NotFound();
            }

            _mapper.Map(episodeToEdit, currentEpisode);

            await _mediaService.EditMovie(id, currentEpisode);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(currentEpisode);
        }

        // -------------------- delete

        [HttpDelete]
        [Route("delete_media")]
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
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MyMDb.DTOs;
using MyMDb.Models;
using MyMDb.ServiceInterfaces;
using MyMDb.Data;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.InteropServices;
using System.Reflection;

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
        [Route("movies_and_series")]
        public async Task<IActionResult> GetMoviesAndSeries() 
        {
            var allMedia = await _mediaService.GetAllMedia();
            var mediaDtos = _mapper.Map<List<MediaDto>>(allMedia);

            Console.WriteLine($"\n\nCurrent dir: {Directory.GetCurrentDirectory()}\n\n");

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
        [Route("media/{id}")]
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
        //[Authorize(Roles = "Admin")]
        [Route("add_movie")]
        public async Task<IActionResult> AddMovie([FromForm] MovieDto movie, IFormFile? poster, IFormFile? video)
        {
            var newMovie = _mapper.Map<Movie>(movie);

            if (poster != null)
            {
                if(!Extensions.IsImageFile(poster.FileName))
                {
                    return BadRequest("Not an image file provided for poster.");
                }
                if (newMovie.PosterPath == null)
                {
                    return BadRequest("No path provided for poster.");
                }

                newMovie.PosterPath =  Paths.ImagesPath + newMovie.PosterPath;

                using (var stream = new FileStream(Paths.Root + newMovie.PosterPath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }
            }

            if (video != null)
            {
                if (!Extensions.IsVideoFile(video.FileName))
                {
                    return BadRequest("Not a video file provided for video.");
                }
                if(newMovie.VideoPath == null) 
                {
                    return BadRequest("No path provided for video.");
                }

                newMovie.VideoPath =  Paths.VideosPath + newMovie.VideoPath;

                using (var stream = new FileStream(Paths.Root + newMovie.VideoPath, FileMode.Create))
                {
                    await video.CopyToAsync(stream);
                }
            }

            newMovie = await _mediaService.AddMovie(newMovie);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newMovie);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [Route("add_series")]
        public async Task<IActionResult> AddSeries(SeriesDto series,  IFormFile? poster)
        {
            var newSeries = _mapper.Map<Series>(series);

            if (poster != null)
            {
                if (!Extensions.IsImageFile(poster.FileName))
                {
                    return BadRequest("Not an image file provided for poster.");
                }
                if (series.PosterPath == null)
                {
                    return BadRequest("No path provided for poster");
                }

                var seriesImagesDirectory = Path.Combine(Paths.Root, Paths.ImagesPath, newSeries.Title + DateTime.Now.ToString());

                newSeries.PosterPath =  Paths.ImagesPath + newSeries.PosterPath;

                using (var stream = new FileStream(Paths.Root + series.PosterPath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }
            }

            newSeries = await _mediaService.AddSeries(newSeries);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newSeries);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [Route("add_episode")]
        public async Task<IActionResult> AddEpisode(EpisodeDto episode,  IFormFile? poster,  IFormFile? video)
        {
            var newEpisode = _mapper.Map<Episode>(episode);

            if (poster != null)
            {
                if (!Extensions.IsImageFile(poster.FileName))
                {
                    return BadRequest("Not an image file provided for poster.");
                }
                if (episode.PosterPath == null)
                {
                    return BadRequest("No path provided for poster");
                }

                newEpisode.PosterPath =  Paths.ImagesPath + newEpisode.PosterPath;

                using (var stream = new FileStream(Paths.Root + episode.PosterPath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }
            }

            if (video != null)
            {
                if (!Extensions.IsImageFile(video.FileName))
                {
                    return BadRequest("Not a video file provided for video.");
                }
                if (episode.VideoPath == null)
                {
                    return BadRequest("No path provided for video");
                }

                newEpisode.VideoPath =  Paths.VideosPath + newEpisode.VideoPath;

                using (var stream = new FileStream(Paths.Root + episode.VideoPath, FileMode.Create))
                {
                    await video.CopyToAsync(stream);
                }
            }

            newEpisode = await _mediaService.AddEpisode(newEpisode);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newEpisode);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [Route("add_many_episodes")]
        public async Task<IActionResult> AddManyEpisodes(Guid seriesId, int seasonNumber, int episodesNumber)
        {
            var series = await _mediaService.GetSeriesById(seriesId);

            if (series == null)
            {
                return NotFound("Series not found");
            }

            var newEpisodes = await _mediaService.AddManyEpisodesToASeries(seriesId, seasonNumber, episodesNumber, series.PosterPath);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newEpisodes);
        }

        //// -------------------- file upload

        //[HttpPost]
        //[DisableRequestSizeLimit] // allows big uploads
        //[Route("{id}/upload_movie_content")]
        //public async Task<IActionResult> UploadMovieContent(Guid id, IFormFile? poster, IFormFile? video)
        //{
        //    if (poster == null && video == null)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    var movie = await _mediaService.GetMovieById(id);
        //    if (movie == null)
        //    {
        //        return NotFound();
        //    }

        //    if (poster != null)
        //    {
        //        var posterPath = Path.Combine(DefaultValues.IMAGES_PATH, movie.Id.ToString()) +
        //                         Path.GetExtension(poster.FileName);
        //        using (var stream = new FileStream(posterPath, FileMode.Create))
        //        {
        //            await poster.CopyToAsync(stream);
        //        }
        //        movie.PosterPath = posterPath;
        //    }

        //    if (video != null)
        //    {
        //        var videoPath = Path.Combine(DefaultValues.VIDEOS_PATH, movie.Id.ToString()) +
        //                        Path.GetExtension(video.FileName);
        //        using (var stream = new FileStream(videoPath, FileMode.Create))
        //        {
        //            await video.CopyToAsync(stream);
        //        }
        //        movie.VideoPath = videoPath;
        //    }

        //    await _mediaService.EditMovie(id, movie);

        //    return Ok(movie);
        //}

        //[HttpPost]
        //[Route("{id}/upload_series_poster")]
        //public async Task<IActionResult> UploadSeriesPoster(Guid id, IFormFile? poster)
        //{
        //    if (poster == null || poster.Length == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    var series = await _mediaService.GetSeriesById(id);
        //    if (series == null)
        //    {
        //        return NotFound();
        //    }

        //    var posterPath = Path.Combine(DefaultValues.IMAGES_PATH, series.Id.ToString()) +
        //                     Path.GetExtension(poster.FileName);

        //    using (var stream = new FileStream(posterPath, FileMode.Create))
        //    {
        //        await poster.CopyToAsync(stream);
        //    }

        //    series.PosterPath = posterPath;

        //    await _mediaService.EditSeries(id, series);

        //    return Ok(series);
        //}

        //[HttpPost]
        //[Route("{id}/upload_episode_content")]
        //public async Task<IActionResult> UploadEpisodeContent(Guid id, IFormFile? poster, IFormFile? video)
        //{
        //    if (poster == null && video == null)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    var episode = await _mediaService.GetEpisodeById(id);
        //    if (episode == null)
        //    {
        //        return NotFound();
        //    }

        //    if (poster != null)
        //    {
        //        var posterPath = Path.Combine(DefaultValues.IMAGES_PATH, episode.SeriesId.ToString(), episode.Id.ToString()) +
        //                         Path.GetExtension(poster.FileName);
        //        using (var stream = new FileStream(posterPath, FileMode.Create))
        //        {
        //            await poster.CopyToAsync(stream);
        //        }
        //        episode.PosterPath = posterPath;
        //    }

        //    if (video != null)
        //    {
        //        var videoPath = Path.Combine(DefaultValues.VIDEOS_PATH, episode.SeriesId.ToString(), episode.Id.ToString()) +
        //                        Path.GetExtension(video.FileName);
        //        using (var stream = new FileStream(videoPath, FileMode.Create))
        //        {
        //            await video.CopyToAsync(stream);
        //        }
        //        episode.VideoPath = videoPath;
        //    }


        //    await _mediaService.EditEpisode(id, episode);

        //    return Ok(episode);
        //}

        // -------------------- edit

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("edit_movie/{id}")]
        public async Task<IActionResult> EditMovie(Guid id, [FromBody] MovieDto movieToEdit, IFormFile? poster, IFormFile? video)
        {
            if (id != movieToEdit.Id)
            {
                return BadRequest();
            }

            var currentMovie = await _mediaService.GetMovieById(id);
            if(currentMovie == null)
            {
                return NotFound();
            }

            if (poster != null)
            {
                if (movieToEdit.PosterPath == null)
                {
                    return BadRequest("No path provided for poster");
                }
                using (var stream = new FileStream(movieToEdit.PosterPath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }
                currentMovie.PosterPath = movieToEdit.PosterPath;
            }

            if (video != null)
            {
                if (movieToEdit.VideoPath == null)
                {
                    return BadRequest("No path provided for video");
                }
                using (var stream = new FileStream(movieToEdit.VideoPath, FileMode.Create))
                {
                    await video.CopyToAsync(stream);
                }
                currentMovie.VideoPath = movieToEdit.VideoPath;
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
        [Authorize(Roles = "Admin")]
        [Route("edit_series/{id}")]
        public async Task<IActionResult> EditSeries(Guid id, [FromBody] SeriesDto seriesToEdit, IFormFile? poster)
        {
            if (id != seriesToEdit.Id)
            {
                return BadRequest();
            }

            var currentSeries = await _mediaService.GetSeriesById(id);
            if (currentSeries == null)
            {
                return NotFound();
            }

            if (poster != null)
            {
                if (seriesToEdit.PosterPath == null)
                {
                    return BadRequest("No path provided for poster");
                }
                using (var stream = new FileStream(seriesToEdit.PosterPath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }
                currentSeries.PosterPath = seriesToEdit.PosterPath;
            }

            _mapper.Map(seriesToEdit, currentSeries);

            await _mediaService.EditSeries(id, currentSeries);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(currentSeries);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("edit_episode/{id}")]
        public async Task<IActionResult> EditEpisode(Guid id, [FromBody] EpisodeDto episodeToEdit, IFormFile? poster, IFormFile? video)
        {
            if (id != episodeToEdit.Id)
            {
                return BadRequest();
            }

            var currentEpisode = await _mediaService.GetEpisodeById(id);
            if (currentEpisode == null)
            {
                return NotFound();
            }

            if (poster != null)
            {
                if(episodeToEdit.PosterPath == null) 
                {
                    return BadRequest("No path provided for poster");
                }
                using (var stream = new FileStream(episodeToEdit.PosterPath, FileMode.Create))
                {
                    await poster.CopyToAsync(stream);
                }
                currentEpisode.PosterPath = episodeToEdit.PosterPath;
            }

            if (video != null)
            {
                if (episodeToEdit.VideoPath == null)
                {
                    return BadRequest("No path provided for video");
                }
                using (var stream = new FileStream(episodeToEdit.VideoPath, FileMode.Create))
                {
                    await video.CopyToAsync(stream);
                }
                currentEpisode.VideoPath = episodeToEdit.VideoPath;
            }

            _mapper.Map(episodeToEdit, currentEpisode);

            await _mediaService.EditEpisode(id, currentEpisode);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(currentEpisode);
        }

        // -------------------- delete

        [HttpDelete]
        //[Authorize(Roles = "Admin")]
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

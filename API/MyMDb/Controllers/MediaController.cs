using Microsoft.AspNetCore.Mvc;
using MyMDb.ServiceInterfaces;
using MyMDb.Services;

namespace MyMDb.Controllers
{
    [ApiController]
    [Route("api/movies")]
    public class MediaController : ControllerBase 
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService) 
        {
            _mediaService = mediaService;
        }

        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllMedia() 
        {
            var allMedia = await _mediaService.GetAllMedia();

            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            return Ok(allMedia);
        }

        [HttpPost]
        [Route("add movie")]
        public async Task<IActionResult> AddMovie(string? title, string? description, DateTime? releaseDate, string? posterPath, string? videoPath)
        {
            var newMovie = await _mediaService.AddMovie(title, description, releaseDate, posterPath, videoPath);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newMovie);
        }

        [HttpPost]
        [Route("add series")]
        public async Task<IActionResult> AddSeries(string? title, string? description, DateTime? releaseDate, string? posterPath)
        {
            var newSeries = await _mediaService.AddSeries(title, description, releaseDate, posterPath);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(newSeries);
        }

    }
}

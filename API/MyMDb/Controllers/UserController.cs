using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMDb.DTOs;
using MyMDb.ServiceInterfaces;

namespace MyMDb.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize]
        [Route("add_movie_review")]
        public async Task<IActionResult> AddMovieReview(string userId, Guid movieId, ReviewDto review)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize]
        [Route("add_series_review")]
        public async Task<IActionResult> AddSeriesReview(string userId, Guid seriesId, ReviewDto review)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize]
        [Route("add_episode_review")]
        public async Task<IActionResult> AddEpisodeReview(string userId, Guid episodeId, ReviewDto review)
        {
            throw new NotImplementedException();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyMDb.DTOs;
using MyMDb.Models;
using MyMDb.ServiceInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyMDb.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AccountController(IUserService userService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser { UserName = userDto.Email, Email = userDto.Email};
            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded) 
            {
                foreach (var error in result.Errors) 
                {
                    ModelState.AddModelError("Errors", error.Description);
                    return BadRequest(ModelState);
                }
            }

            result = await _userManager.AddToRoleAsync(user, "user");

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("Errors", error.Description);
                    return BadRequest(ModelState);
                }
            }

            return Ok(new { Result = "User created successfully" });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(userDto.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Email not found" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, userDto.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Wrong password" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("JWT key is not configured.");
            }

            var key = Encoding.ASCII.GetBytes(keyString);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, userDto.Email)
            };

            // Adaugă fiecare rol în lista de revendicări
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Issuer"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
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

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyMDb.Data;
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
        private readonly IReviewService _reviewService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(IUserService userService, IReviewService reviewService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _reviewService = reviewService;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (userDto.Email == null)
            {
                return BadRequest("No email provided for the user");
            }

            if (userDto.Password == null)
            {
                return BadRequest("No password provided for the user");
            }

            if (await _userManager.FindByEmailAsync(userDto.Email) != null)
            {
                return BadRequest("There is already an user with this email");
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

            var userProfile = new UserProfile();
            userProfile.UserId = user.Id;
            
            userProfile = await _userService.CreateUserProfileAsync(userProfile);
            
            if (userProfile == null)
            {
                return BadRequest("Unknown error creating profile");
            }

            return Ok(new { Result = "User created successfully" });
        }

        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return NotFound();
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userProfile = await _userService.GetUserProfileAsync(userId);

            if (userProfile == null) 
            { 
                return NotFound();
            }

            return Ok(userProfile);
        }

        [HttpPost]
        [Authorize]
        [Route("edit_profile")]
        public async Task<IActionResult> EditProfile([FromForm] ProfileDto userProfile, IFormFile? profilePic)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return Forbid();
            }

            var loggedUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            if (loggedUserId != userProfile.UserId || loggedUserId == null)
            {
                return Forbid();
            }

            var user = await _userService.GetUserProfileAsync(loggedUserId);
            if (user == null) 
            {
                return NotFound();
            }

            if (profilePic != null) 
            {
                if (!Extensions.IsImageFile(profilePic.FileName))
                {
                    return BadRequest("Not an image file provided.");
                }
                if(userProfile.ProfilePicPath == null)
                {
                    return BadRequest("No path provided for profile pic");
                }

                userProfile.ProfilePicPath = Paths.ProfilePath + userProfile.ProfilePicPath;
                Console.WriteLine("\n\n" + userProfile.ProfilePicPath + "\n\n");
                using (var stream = new FileStream(Paths.Root + userProfile.ProfilePicPath, FileMode.Create))
                {
                    await profilePic.CopyToAsync(stream);
                }
            }

            var updatedProfile = await _userService.EditUserProfileAsync(loggedUserId, userProfile);
            
            if (updatedProfile == null)
            {
                return BadRequest("Profile editing failed");
            }

            return Ok(updatedProfile);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (userDto.Email == null || userDto.Password == null)
            {
                return NotFound();
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
        [Authorize("user")]
        [Route("add_review")]
        public async Task<IActionResult> AddMovieReview([FromBody] ReviewDto review)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return Forbid();
            }

            var loggedUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (loggedUserId == null)
            {
                return Forbid("You need to be logged in to review a media");
            }

            var existentReview = await _reviewService.GetByUserAsync(loggedUserId, review.mediaId);

            if (existentReview != null) 
            {
                return BadRequest("Media already reviewed");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var adddedReview = await _reviewService.AddReview(loggedUserId, review);

            if (adddedReview == null)
            {
                return BadRequest("Review adding failed");
            }

            return Ok(adddedReview);
        }

        [HttpDelete]
        [Authorize("user")]
        [Route("delete_review/{mediaId}")]
        public async Task<IActionResult> DeleteMovieReview(Guid mediaId)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return Forbid();
            }

            var loggedUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (loggedUserId == null)
            {
                return Forbid("You need to be logged in to review a media");
            }

            var review = await _reviewService.GetByUserAsync(loggedUserId, mediaId);

            if (review == null)
            {
                return NotFound("No review to delete");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _reviewService.DeleteReview(review);

            return Ok();
        }
    }
}

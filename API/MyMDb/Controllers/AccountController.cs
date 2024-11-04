using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyMDb.Data;
using MyMDb.DTOs;
using MyMDb.Models;
using MyMDb.ServiceInterfaces;
using System.Security.Claims;

namespace MyMDb.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IReviewService _reviewService;
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, IReviewService reviewService, ITokenService tokenService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configuration, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<AccountController> logger)
        {
            _userService = userService;
            _reviewService = reviewService;
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            _logger.LogInformation("Login attempt for user: {Email}", userDto.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for user: {Email}", userDto.Email);
                return BadRequest(ModelState);
            }

            if (userDto.Email == null || userDto.Password == null)
            {
                _logger.LogWarning("Email or password not provided for login attempt");
                return NotFound();
            }

            var user = await _userManager.FindByEmailAsync(userDto.Email);
            if (user == null)
            {
                _logger.LogWarning("User with email {Email} not found", userDto.Email);
                return Unauthorized(new { Message = "Email not found" });
            }
            if (!user.Approved)
            {
                _logger.LogWarning("User with email {Email} not approved", userDto.Email);
                return Unauthorized(new { Message = "User not approved" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, userDto.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed login attempt for user: {Email}", userDto.Email);
                return Unauthorized(new { Message = "Wrong password" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateAccessToken(user, roles);

            var refreshToken = _tokenService.GenerateRefreshToken();
            await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

            _logger.LogInformation("Login successful for user: {Email}", userDto.Email);
            return Ok(new { Token = token, RefreshToken = refreshToken });
        }

        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshDto refreshDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = refreshDto.Token;
            var refreshToken = refreshDto.RefreshToken;

            if (token == null || refreshToken == null)
            {
                return BadRequest(new { Message = "Token or refresh token not provided" });
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(token);
            var userId = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userId == null || !await _tokenService.ValidateRefreshTokenAsync(userId, refreshToken))
            {
                return Unauthorized(new { Message = "Invalid refresh token" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound("User from token not found!");
            }
            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _tokenService.DeleteRefreshTokenAsync(userId, refreshToken);
            await _tokenService.SaveRefreshTokenAsync(userId, newRefreshToken);

            return Ok(new { Token = newAccessToken, RefreshToken = newRefreshToken });
        }

        [HttpGet]
        [Authorize]
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

                userProfile.ProfilePicPath = _configuration["Paths:ProfilePics"] + userProfile.ProfilePicPath;
                
                using (var stream = new FileStream(_configuration["Paths:Root"] + userProfile.ProfilePicPath, FileMode.Create))
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
        [Authorize]
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
        [Authorize]
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

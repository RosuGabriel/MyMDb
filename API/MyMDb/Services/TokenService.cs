using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MyMDb.Models;
using MyMDb.RepositoryInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyMDb.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration, ITokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
            _configuration = configuration;
        }

        public async Task DeleteRefreshTokenAsync(string userId, string refreshToken)
        {
            var userRefreshToken = await _tokenRepository.GetRefreshTokenAsync(userId, refreshToken);
            if (userRefreshToken != null)
            {
                await _tokenRepository.Delete(userRefreshToken);
            }
        }

        public string GenerateAccessToken(IdentityUser user, IList<string> roles)
        {
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
            new Claim(ClaimTypes.Email, user.Email!)
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Issuer"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            var userRefreshToken = new UserRefreshToken
            {
                UserId = userId,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddDays(7)
            };

            await _tokenRepository.AddAsync(userRefreshToken);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var userRefreshToken = await _tokenRepository.GetRefreshTokenAsync(userId, refreshToken);

            return userRefreshToken != null && userRefreshToken.Expiration > DateTime.UtcNow;
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                var jwtToken = securityToken as JwtSecurityToken;
                if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}

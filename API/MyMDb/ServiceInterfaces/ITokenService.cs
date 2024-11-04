using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public interface ITokenService
{
    public string GenerateAccessToken(IdentityUser user, IList<string> roles);
    public string GenerateRefreshToken();
    public Task SaveRefreshTokenAsync(string userId, string refreshToken);
    public Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
    public Task DeleteRefreshTokenAsync(string userId, string refreshToken);
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

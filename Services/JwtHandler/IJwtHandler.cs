using LinguacApi.Services.JwtHandler;
using System.Security.Claims;

namespace LinguacApi.Services
{
    public interface IJwtHandler
    {
        TokenResult GenerateAccessToken(string userId, IEnumerable<string> roles);

        TokenResult GenerateRefreshToken(string userId);

        ClaimsPrincipal? ValidateAccessToken(string accessToken);

        ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
    }
}

using LinguacApi.Services.JwtHandler;
using System.Security.Claims;

namespace LinguacApi.Services
{
    public interface IJwtHandler
    {
        TokenResult GenerateAccessToken(Guid userId, IEnumerable<string> roles);

        TokenResult GenerateRefreshToken(Guid userId);

        ClaimsPrincipal? ValidateAccessToken(string accessToken);

        ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
    }
}

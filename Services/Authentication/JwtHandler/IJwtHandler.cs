using System.Security.Claims;

namespace LinguacApi.Services.Authentication.JwtHandler
{
	public interface IJwtHandler
	{
		TokenResult GenerateAccessToken(Guid userId, IEnumerable<string> roles);

		TokenResult GenerateRefreshToken(Guid userId);

		ClaimsPrincipal? ValidateAccessToken(string accessToken);

		ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
	}
}

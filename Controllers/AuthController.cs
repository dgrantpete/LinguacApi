using LinguacApi.Configurations;
using LinguacApi.Data.Dtos;
using LinguacApi.Services;
using LinguacApi.Services.JwtHandler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace LinguacApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController(IJwtHandler jwtHandler, IOptions<JwtConfiguration> jwtConfiguration) : ControllerBase
    {
        private readonly JwtConfiguration _jwtConfiguration = jwtConfiguration.Value;

        [AllowAnonymous]
        [HttpPost("[action]")]
        public ActionResult<TokenStatusDto> Login()
        {
            TokenResult accessToken = jwtHandler.GenerateAccessToken("userId", ["user"]);

            AddAccessTokenCookie(accessToken.Expiration, accessToken.Value);

            TokenResult refreshToken = jwtHandler.GenerateRefreshToken("userId");

            AddRefreshTokenCookie(refreshToken.Expiration, refreshToken.Value);

            return Ok(new TokenStatusDto() { AccessTokenExpiration = accessToken.Expiration, RefreshTokenExpiration = refreshToken.Expiration });
        }

        [Authorize(AuthenticationSchemes = "RefreshCookieJwt")]
        [HttpGet("[action]")]
        public ActionResult<TokenStatusDto> Refresh()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
            {
                return Unauthorized();
            }

            TokenResult accessToken = jwtHandler.GenerateAccessToken(userId, ["role"]);

            AddRefreshTokenCookie(accessToken.Expiration, accessToken.Value);

            return Ok(new TokenStatusDto() { AccessTokenExpiration = accessToken.Expiration });
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public ActionResult Logout()
        {
            AddAccessTokenCookie(DateTime.UtcNow.AddDays(-1));
            AddRefreshTokenCookie(DateTime.UtcNow.AddDays(-1));
            return Ok();
        }

        [HttpPost("[action]")]
        public ActionResult Register()
        {
            return Ok();
        }

        private void AddAccessTokenCookie(DateTime expiration, string token = "")
        {
            Response.Cookies.Append(_jwtConfiguration.AccessCookieName, token, CreateTokenCookieOptions(expiration, _jwtConfiguration.AccessCookieDomain));
        }

        private void AddRefreshTokenCookie(DateTime expiration, string token = "")
        {
            // Getting endpoint for current controller so all auth controller requests recieve refresh token cookie
            // Couldn't find a cleaner way to do this
            string authEndpoint = string.Join('/', Url.Action(null, ControllerContext.ActionDescriptor.ControllerName)!
                .Split('/')
                .SkipLast(1));

            Response.Cookies.Append(_jwtConfiguration.RefreshCookieName, token, CreateTokenCookieOptions(expiration, _jwtConfiguration.RefreshCookieDomain, authEndpoint));
        }

        private static CookieOptions CreateTokenCookieOptions(DateTime expiration, string domain, string? path = default) => new()
        {
            HttpOnly = true,
            Secure = true,
            IsEssential = true,
            Domain = domain,
            SameSite = SameSiteMode.Strict,
            Expires = expiration,
            Path = path ?? "/"
        };
    }
}

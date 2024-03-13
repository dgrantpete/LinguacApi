using LinguacApi.Configurations;
using LinguacApi.Data.Binders;
using LinguacApi.Data.Database;
using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;
using LinguacApi.Services;
using LinguacApi.Services.JwtHandler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LinguacApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthController(IJwtHandler jwtHandler,
        IOptions<JwtConfiguration> jwtConfiguration,
        PasswordHasher<User> passwordHasher,
        LinguacDbContext dbContext) : ControllerBase
    {
        private readonly JwtConfiguration _jwtConfiguration = jwtConfiguration.Value;

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<TokenStatusDto>> Login(LoginDto loginInfo)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Email == loginInfo.Email);

            if (user is null || !await VerifyPassword(user, loginInfo.Password))
            {
                return BadRequest("Invalid email or password");
            }

            TokenResult accessToken = jwtHandler.GenerateAccessToken(user.Id, user.Roles);

            AddAccessTokenCookie(accessToken.Expiration, accessToken.Value);

            TokenResult refreshToken = jwtHandler.GenerateRefreshToken(user.Id);

            AddRefreshTokenCookie(refreshToken.Expiration, refreshToken.Value);

            return Ok(new TokenStatusDto(accessToken.Expiration, refreshToken.Expiration));
        }

        [Authorize(AuthenticationSchemes = "RefreshCookieJwt")]
        [HttpPost]
        public ActionResult<TokenStatusDto> Refresh([AuthenticatedUser] User user)
        {
            TokenResult accessToken = jwtHandler.GenerateAccessToken(user.Id, user.Roles);

            AddAccessTokenCookie(accessToken.Expiration, accessToken.Value);

            return Ok(new AccessTokenStatusDto(accessToken.Expiration));
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Logout()
        {
            AddAccessTokenCookie(DateTime.UtcNow.AddDays(-1));
            AddRefreshTokenCookie(DateTime.UtcNow.AddDays(-1));
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Register(AccountRegistrationDto accountRegistrationInfo)
        {
            if (await dbContext.Users.AnyAsync(user => user.Email == accountRegistrationInfo.Email))
            {
                return BadRequest("Email already in use");
            }

            User user = new()
            {
                Email = accountRegistrationInfo.Email
            };

            user.PasswordHash = passwordHasher.HashPassword(user, accountRegistrationInfo.Password);

            await dbContext.AddAsync(user);

            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword([AuthenticatedUser] User user, ChangePasswordDto changePasswordInfo)
        {
            if (!await VerifyPassword(user, changePasswordInfo.OldPassword))
            {
                return BadRequest("Old password is incorrect");
            }

            user.PasswordHash = passwordHasher.HashPassword(user, changePasswordInfo.NewPassword);

            await dbContext.SaveChangesAsync();

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

        private async Task<bool> VerifyPassword(User user, string password)
        {
            if (user.PasswordHash is null)
            {
                return false;
            }

            PasswordVerificationResult passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (passwordVerificationResult is PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = passwordHasher.HashPassword(user, password);
                await dbContext.SaveChangesAsync();
            }
            else if (passwordVerificationResult is not PasswordVerificationResult.Success)
            {
                return false;
            }

            return true;
        }
    }
}

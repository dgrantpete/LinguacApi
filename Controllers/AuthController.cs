using LinguacApi.Configurations;
using LinguacApi.Data.Binders;
using LinguacApi.Data.Database;
using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;
using LinguacApi.Services.EmailConfirmer;
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
        LinguacDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        EmailConfirmer emailConfirmer) : ControllerBase
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
            AddRefreshTokenExpirationCookie(refreshToken.Expiration);

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
            var expiredTime = DateTime.UtcNow.AddDays(-1);

            AddAccessTokenCookie(expiredTime);
            AddRefreshTokenCookie(expiredTime);
            AddRefreshTokenExpirationCookie(expiredTime);
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Register(RegistrationDto accountRegistrationInfo)
        {
            string normalizedEmail = accountRegistrationInfo.Email
                .Trim()
                .ToLower();

            if (await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail))
            {
                return BadRequest("Email already in use");
            }

            User user = new()
            {
                Email = normalizedEmail
            };

            Guid emailConfirmationId = Guid.NewGuid();

            DateTime emailConfirmationExpiration = await emailConfirmer.SendConfirmationEmail(normalizedEmail, emailConfirmationId);

            PendingEmailConfirmation pendingEmailConfirmation = new
            (
                normalizedEmail,
                passwordHasher.HashPassword(user, accountRegistrationInfo.Password),
                emailConfirmationExpiration
            )
            {
                Id = emailConfirmationId
            };

            await dbContext.AddAsync(pendingEmailConfirmation);

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

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ConfirmEmail(EmailConfirmationDto emailConfirmationInfo)
        {
            Guid confirmationId = emailConfirmationInfo.ConfirmationId;

            PendingEmailConfirmation? pendingEmailConfirmation = await dbContext.FindAsync<PendingEmailConfirmation>(confirmationId);

            if (pendingEmailConfirmation is null)
            {
                return BadRequest("Invalid confirmation id");
            }

            dbContext.Remove(pendingEmailConfirmation);

            if (pendingEmailConfirmation.ExpiresAt < DateTime.UtcNow)
            {
                await dbContext.SaveChangesAsync();
                return BadRequest("Confirmation has expired");
            }

            if (await dbContext.Users.AnyAsync(user => user.Email == pendingEmailConfirmation.Email))
            {
                await dbContext.SaveChangesAsync();
                return BadRequest("Email already in use");
            }

            User user = new()
            {
                Email = pendingEmailConfirmation.Email,
                PasswordHash = pendingEmailConfirmation.PasswordHash
            };

            await dbContext.AddAsync(user);

            await dbContext.SaveChangesAsync();

            return Ok();
        }

        private void AddRefreshTokenExpirationCookie(DateTime expiration)
        {
            Response.Cookies.Append("refreshTokenExpiration", expiration.ToString("O"), CreateTokenCookieOptions(expiration, _jwtConfiguration.RefreshCookieDomain, httpOnly: false));
        }

        private void AddAccessTokenCookie(DateTime expiration, string token = "")
        {
            Response.Cookies.Append(_jwtConfiguration.AccessCookieName, token, CreateTokenCookieOptions(expiration, _jwtConfiguration.AccessCookieDomain));
        }

        private void AddRefreshTokenCookie(DateTime expiration, string token = "")
        {
            string refreshEndpoint = Url.Action(nameof(Refresh), ControllerContext.ActionDescriptor.ControllerName)!;

            Response.Cookies.Append(_jwtConfiguration.RefreshCookieName, token, CreateTokenCookieOptions(expiration, _jwtConfiguration.RefreshCookieDomain, refreshEndpoint));
        }

        private static CookieOptions CreateTokenCookieOptions(DateTime expiration, string domain, string? path = default, bool httpOnly = true) => new()
        {
            HttpOnly = httpOnly,
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

using LinguacApi.Configurations;
using LinguacApi.Data.Binders;
using LinguacApi.Data.Database;
using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;
using LinguacApi.Services.Authentication;
using LinguacApi.Services.Authentication.TokenHandler;
using LinguacApi.Services.Email.Confirmation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LinguacApi.Controllers
{
	[ApiController]
	[Route("[controller]/[action]")]
	public class AuthController(
		IOptions<AuthTokenConfiguration> tokenConfiguration,
		ITokenHandler tokenHandler,
		LinguacDbContext dbContext,
		IPasswordHasher<User> passwordHasher,
		EmailConfirmer emailConfirmer) : ControllerBase
	{
		private readonly AuthTokenConfiguration tokenConfiguration = tokenConfiguration.Value;

		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult<TokenStatusDto>> PostLogin(LoginDto loginInfo)
		{
			string normalizedEmail = NormalizeEmail(loginInfo.Email);

			var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Email == normalizedEmail);

			if (user is null || !await VerifyPassword(user, loginInfo.Password))
			{
				return BadRequest("Invalid email or password");
			}

			TokenResult accessToken = tokenHandler.GenerateAccessToken(user.Id, user.Roles);

			AddAccessTokenCookie(accessToken.Expiration, accessToken.Value);

			TokenResult refreshToken = tokenHandler.GenerateRefreshToken(user.Id);

			AddRefreshTokenCookie(refreshToken.Expiration, refreshToken.Value);
			AddRefreshTokenExpirationCookie(refreshToken.Expiration);

			return Ok(new TokenStatusDto(accessToken.Expiration, refreshToken.Expiration));
		}

		[Authorize(AuthenticationSchemes = AuthenticationSchemes.RefreshToken)]
		[HttpPost]
		public ActionResult<TokenStatusDto> Refresh([AuthenticatedUser] User user)
		{
			TokenResult accessToken = tokenHandler.GenerateAccessToken(user.Id, user.Roles);

			AddAccessTokenCookie(accessToken.Expiration, accessToken.Value);

			return Ok(new AccessTokenStatusDto(accessToken.Expiration));
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult PostLogout()
		{
			var expiredTime = DateTime.UtcNow.AddDays(-1);

			AddAccessTokenCookie(expiredTime);
			AddRefreshTokenCookie(expiredTime);
			AddRefreshTokenExpirationCookie(expiredTime);
			return Ok();
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult> PostRegister(RegistrationDto accountRegistrationInfo)
		{
			string normalizedEmail = NormalizeEmail(accountRegistrationInfo.Email);

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
		[AllowAnonymous]
		public async Task<ActionResult> PostForgotPassword(ForgotPasswordDto forgotPasswordInfo)
		{
			string normalizedEmail = NormalizeEmail(forgotPasswordInfo.Email);

			User? user = await dbContext.Users.FirstOrDefaultAsync(user => user.Email == normalizedEmail);

			if (user is null)
			{
				return BadRequest("Email not found");
			}
			
			Guid passwordResetId = Guid.NewGuid();

			DateTime passwordResetExpiration = await emailConfirmer.SendPasswordResetEmail(normalizedEmail, passwordResetId);

			PendingPasswordReset pendingPasswordReset = new(passwordResetExpiration) { User = user, Id = passwordResetId };

			await dbContext.AddAsync(pendingPasswordReset);

			await dbContext.SaveChangesAsync();

			return Ok();
		}

		[HttpGet("{passwordResetId}")]
		[AllowAnonymous]
		public async Task<ActionResult<CheckPasswordResetResponseDto>> GetResetPassword(Guid passwordResetId)
		{
			PendingPasswordReset? pendingPasswordReset = await dbContext.PasswordResetRequests
				.Include(request => request.User)
				.FirstOrDefaultAsync(request => request.Id == passwordResetId);

			if (pendingPasswordReset is null)
			{
				return BadRequest("Invalid confirmation id");
			}

			if (pendingPasswordReset.ExpiresAt < DateTime.UtcNow)
			{
				return BadRequest("Confirmation has expired");
			}

			return Ok(new CheckPasswordResetResponseDto(pendingPasswordReset.User.Email));
		}

		[HttpPost("{passwordResetId}")]
		[AllowAnonymous]
		public async Task<ActionResult> PostResetPassword(ResetPasswordDto resetPasswordInfo, Guid passwordResetId)
		{
			PendingPasswordReset? pendingPasswordReset = await dbContext.PasswordResetRequests
				.Include(request => request.User)
				.FirstOrDefaultAsync(request => request.Id == passwordResetId);

			if (pendingPasswordReset is null)
			{
				return BadRequest("Invalid confirmation id");
			}

			dbContext.Remove(pendingPasswordReset);

			if (pendingPasswordReset.ExpiresAt < DateTime.UtcNow)
			{
				await dbContext.SaveChangesAsync();
				return BadRequest("Confirmation has expired");
			}

			User user = pendingPasswordReset.User;

			user.PasswordHash = passwordHasher.HashPassword(user, resetPasswordInfo.NewPassword);

			await dbContext.SaveChangesAsync();

			return Ok();
		}

		[AllowAnonymous]
		[HttpPost]
		public async Task<ActionResult> PostConfirmEmail(EmailConfirmationDto emailConfirmationInfo)
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
			Response.Cookies.Append("refreshTokenExpiration", expiration.ToString("O"), CreateTokenCookieOptions(expiration, tokenConfiguration.AccessCookieDomain));
		}

		private void AddAccessTokenCookie(DateTime expiration, string token = "")
		{
			Response.Cookies.Append(tokenConfiguration.AccessCookieName, token, CreateTokenCookieOptions(expiration, tokenConfiguration.AccessCookieDomain));
		}

		private void AddRefreshTokenCookie(DateTime expiration, string token = "")
		{
			// Getting endpoint for current controller so all auth controller requests recieve refresh token cookie
			// Couldn't find a cleaner way to do this
			string authEndpoint = '/' + nameof(AuthController).Replace("Controller", string.Empty).ToLower();

			Response.Cookies.Append(tokenConfiguration.RefreshCookieName, token, CreateTokenCookieOptions(expiration, tokenConfiguration.RefreshCookieDomain, authEndpoint));
		}

		private static CookieOptions CreateTokenCookieOptions(DateTime expiration, string domain, string? path = default, bool httpOnly = true, bool secure = true) => new()
		{
			HttpOnly = httpOnly,
			Secure = secure,
			IsEssential = true,
			Domain = domain,
			SameSite = SameSiteMode.Strict,
			Expires = expiration,
			Path = path ?? "/"
		};

		private static string NormalizeEmail(string email) => email.Trim().ToLower();

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

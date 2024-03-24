using LinguacApi.Services.Authentication.TokenHandler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace LinguacApi.Services.Authentication.CookieAuthenticationHandler
{
	public class CookieAuthenticationHandler(
		IOptionsMonitor<CookieAuthenticationOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder,
		ITokenHandler tokenHandler) : AuthenticationHandler<CookieAuthenticationOptions>(options, logger, encoder)
	{
		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if (!Request.Cookies.TryGetValue(Options.CookieName, out var token))
			{
				return Task.FromResult(AuthenticateResult.NoResult());
			}

			var principal = Options.ValidateAccessTokenDelegate(tokenHandler, token);

			if (principal is null)
			{
				return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
			}

			var ticket = new AuthenticationTicket(principal, Scheme.Name);

			return Task.FromResult(AuthenticateResult.Success(ticket));
		}
	}
}

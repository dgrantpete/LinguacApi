using LinguacApi.Services.Authentication.TokenHandler;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace LinguacApi.Services.Authentication.CookieAuthenticationHandler
{
	public class CookieAuthenticationOptions : AuthenticationSchemeOptions
	{
		public string CookieName { get; set; } = default!;

		public Func<ITokenHandler, string, ClaimsPrincipal?> ValidateAccessTokenDelegate { get; set; } =
			(tokenHandler, token) => tokenHandler.ValidateAccessToken(token);
	}
}

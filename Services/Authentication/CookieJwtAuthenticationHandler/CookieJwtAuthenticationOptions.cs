using LinguacApi.Configurations;
using LinguacApi.Services.Authentication.JwtHandler;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace LinguacApi.Services.Authentication.CookieJwtAuthenticationHandler
{
	public class CookieJwtAuthenticationOptions : AuthenticationSchemeOptions
	{
		public Func<JwtConfiguration, string> GetCookieNameDelegate { get; set; } = (jwtConfiguration) => jwtConfiguration.AccessCookieName;

		public Func<IJwtHandler, string, ClaimsPrincipal?> ValidateAccessTokenDelegate { get; set; } = (jwtHandler, jwt) => jwtHandler.ValidateAccessToken(jwt);
	}
}

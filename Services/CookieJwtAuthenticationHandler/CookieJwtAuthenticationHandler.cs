using LinguacApi.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace LinguacApi.Services.CookieJwtAuthenticationHandler
{
    public class CookieJwtAuthenticationHandler(
        IOptionsMonitor<CookieJwtAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IJwtHandler jwtHandler,
        IOptions<JwtConfiguration> jwtConfiguration) : AuthenticationHandler<CookieJwtAuthenticationOptions>(options, logger, encoder)
    {
        private readonly JwtConfiguration _jwtConfiguration = jwtConfiguration.Value;

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string cookieName = Options.GetCookieNameDelegate(_jwtConfiguration);

            if (!Request.Cookies.TryGetValue(cookieName, out var jwt))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var principal = Options.ValidateAccessTokenDelegate(jwtHandler, jwt);

            if (principal is null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
            }

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}

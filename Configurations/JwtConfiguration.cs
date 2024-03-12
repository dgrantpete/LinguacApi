namespace LinguacApi.Configurations
{
    public class JwtConfiguration
    {
        required public string AccessSecret { get; set; }

        required public string RefreshSecret { get; set; }

        required public string Issuer { get; set; }

        required public string Audience { get; set; }

        required public int AccessExpirationSeconds { get; set; }

        required public int RefreshExpirationSeconds { get; set; }

        required public string AccessCookieName { get; set; }

        required public string RefreshCookieName { get; set; }

        required public string AccessCookieDomain { get; set; }

        required public string RefreshCookieDomain { get; set; }
    }
}

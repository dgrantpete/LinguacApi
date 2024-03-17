namespace LinguacApi.Configurations
{
    public class JwtConfiguration
    {
        public required string AccessSecret { get; set; }

        public required string RefreshSecret { get; set; }

        public required string Issuer { get; set; }

        public required string Audience { get; set; }

        public required TimeSpan AccessExpirationTime { get; set; }

        public required TimeSpan RefreshExpirationTime { get; set; }

        public required string AccessCookieName { get; set; }

        public required string RefreshCookieName { get; set; }

        public required string AccessCookieDomain { get; set; }

        public required string RefreshCookieDomain { get; set; }
    }
}

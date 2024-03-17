namespace LinguacApi.Configurations
{
    public class EmailConfirmationConfiguration
    {
        public required string ApiKey { get; set; }

        public required string FromEmail { get; set; }

        public required string FromName { get; set; }

        public required string ReplyToEmail { get; set; }

        public required string ReplyToName { get; set; }

        public required string ConfirmationUrl { get; set; }

        public required string Subject { get; set; }

        public required string HtmlContent { get; set; }

        public required string PlainTextContent { get; set; }

        public required TimeSpan ExpirationTime { get; set; }
    }
}

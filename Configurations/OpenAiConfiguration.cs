namespace LinguacApi.Configurations
{
    public class OpenAiConfiguration
    {
        public required string ApiKey { get; set; }

        public required string Model { get; set; }

        public required string EndpointUrl { get; set; }
    }
}

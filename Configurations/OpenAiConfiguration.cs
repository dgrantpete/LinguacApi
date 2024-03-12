namespace LinguacApi.Configurations
{
    public class OpenAiConfiguration
    {
        required public string ApiKey { get; set; }

        required public string Model { get; set; }

        required public string EndpointUrl { get; set; }
    }
}

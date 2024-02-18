namespace LinguacApi.Services.StoryGenerator
{
    public class OpenAiConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;
    }

    public class PromptConfiguration
    {
        public string SystemStoryPrompt { get; set; } = string.Empty;
        public string SystemQuestionPrompt { get; set; } = string.Empty;
    }
}

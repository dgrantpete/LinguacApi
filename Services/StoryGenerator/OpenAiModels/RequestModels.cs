namespace LinguacApi.Services.StoryGenerator.OpenAiModels
{
    public record CompletionRequest(string Model, int MaxTokens, double Temperature, int Seed, ResponseFormat ResponseFormat, IEnumerable<Message> Messages);

    public record ResponseFormat(string Type);

    public record Message(MessageRole Role, string Content);

    public enum MessageRole
    {
        System,
        User,
        Assistant
    }
}

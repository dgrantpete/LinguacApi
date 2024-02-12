namespace LinguacApi.Services.StoryGenerator.OpenAiModels
{
    public record CompletionRequest(
        string Model,
        int MaxTokens,
        double Temperature,
        IEnumerable<Message> Messages,
        int? Seed = null,
        ResponseFormat? ResponseFormat = null
    );

    public record ResponseFormat(string Type);

    public record Message(MessageRole Role, string Content);

    public enum MessageRole
    {
        System,
        User,
        Assistant
    }
}

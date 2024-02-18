namespace LinguacApi.Data.Dtos
{
    public record StoryDto(
        Guid Id,
        string Title,
        string Content,
        Language Language,
        CefrLevel Level
    );

    public record CreateStoryDto(
        Language Language,
        CefrLevel Level,
        string? Prompt = null
    );
}

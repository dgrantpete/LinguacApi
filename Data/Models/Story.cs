namespace LinguacApi.Data.Models
{
    public record Story(Guid Id, string Title, string Content, Language Language, CefrLevel Level)
    {
        public ICollection<Question> Questions { get; init; } = new List<Question>();
    }
}

namespace LinguacApi.Data.Models
{
    public record Question(Guid Id, string Text)
    {
        public required Story Story { get; init; }
    }
}

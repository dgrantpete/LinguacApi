namespace LinguacApi.Data.Models
{
    public record Question(string Text)
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required Story Story { get; init; }

        public ICollection<Answer> Answers { get; init; } = [];
    }
}

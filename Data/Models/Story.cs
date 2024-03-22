namespace LinguacApi.Data.Models
{
	public record Story(string Title, string Content, Language Language, CefrLevel Level)
	{
		public Guid Id { get; init; } = Guid.NewGuid();

		public ICollection<Question> Questions { get; init; } = [];
	}
}

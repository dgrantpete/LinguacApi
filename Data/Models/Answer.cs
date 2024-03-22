namespace LinguacApi.Data.Models
{
	public record Answer(string Text, bool IsCorrect)
	{
		public Guid Id { get; init; } = Guid.NewGuid();

		public required Question Question { get; init; }
	}
}

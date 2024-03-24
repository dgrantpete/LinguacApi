namespace LinguacApi.Data.Models
{
	public record PendingPasswordReset(DateTime ExpiresAt)
	{
		public Guid Id { get; init; } = Guid.NewGuid();

		public required User User { get; init; }
	}
}

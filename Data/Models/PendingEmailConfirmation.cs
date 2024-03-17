namespace LinguacApi.Data.Models
{
    public record PendingEmailConfirmation(string Email, string PasswordHash, DateTime ExpiresAt)
    {
        public Guid Id { get; init; } = Guid.NewGuid();
    }
}

namespace LinguacApi.Data.Models
{
    public record User
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string? Email { get; set; }

        public string? PasswordHash { get; set; }
    }
}

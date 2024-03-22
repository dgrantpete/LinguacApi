using Microsoft.EntityFrameworkCore;

namespace LinguacApi.Data.Models
{
	[Index(nameof(Email), IsUnique = true)]
	public record User
	{
		public Guid Id { get; init; } = Guid.NewGuid();

		public string? Email { get; set; }

		public string? PasswordHash { get; set; }

		public bool IsAdmin { get; set; } = false;

		public IEnumerable<string> Roles => IsAdmin ? ["admin", "user"] : ["user"];
	}
}

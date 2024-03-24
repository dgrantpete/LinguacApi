using LinguacApi.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LinguacApi.Data.Database
{
	public class LinguacDbContext(DbContextOptions<LinguacDbContext> options) : DbContext(options)
	{
		public DbSet<Story> Stories { get; private set; }

		public DbSet<Question> Questions { get; private set; }

		public DbSet<Answer> Answers { get; private set; }

		public DbSet<User> Users { get; private set; }

		public DbSet<PendingEmailConfirmation> PendingEmailConfirmations { get; private set; }

		public DbSet<PendingPasswordReset> PasswordResetRequests { get; private set; }
	}
}

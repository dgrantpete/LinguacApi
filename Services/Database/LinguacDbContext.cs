using LinguacApi.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LinguacApi.Services.Database
{
    public class LinguacDbContext(DbContextOptions<LinguacDbContext> options) : DbContext(options)
    {
        public DbSet<Story> Stories { get; private set; }

        public DbSet<Question> Questions { get; private set; }
    }
}

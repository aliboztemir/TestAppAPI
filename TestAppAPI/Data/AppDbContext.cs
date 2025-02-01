using Microsoft.EntityFrameworkCore;
using TestAppAPI.Models;

namespace TestAppAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<StudyGroup> StudyGroups { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StudyGroup>()
                .HasMany(sg => sg.Users)
                .WithMany();

            base.OnModelCreating(modelBuilder);
        }
    }
}

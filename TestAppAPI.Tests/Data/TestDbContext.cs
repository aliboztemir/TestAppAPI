using Microsoft.EntityFrameworkCore;
using TestAppAPI.Data;

namespace TestAppAPI.Tests.Data
{
    // ✅ TestDbContext now inherits from AppDbContext with correct constructor
    public class TestDbContext : AppDbContext
    {
        public TestDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}

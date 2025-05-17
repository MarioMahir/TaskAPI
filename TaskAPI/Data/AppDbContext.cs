using Microsoft.EntityFrameworkCore;
using TaskAPI.Models;

namespace TaskAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions opts) : base(opts) { }

        public DbSet<Models.Task> Tasks { get; set; }
    }
}

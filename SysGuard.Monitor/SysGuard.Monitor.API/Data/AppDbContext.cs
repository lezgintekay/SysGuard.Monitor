using Microsoft.EntityFrameworkCore;
using SysGuard.Monitor.Models; //

namespace SysGuard.Monitor.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SystemStats> Metrics { get; set; } 
    }
}
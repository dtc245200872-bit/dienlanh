using Microsoft.EntityFrameworkCore;
using dienlanh.Models;

namespace dienlanh.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RepairRequest> RepairRequests { get; set; }
    }
}
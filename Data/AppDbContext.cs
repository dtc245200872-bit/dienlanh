using Microsoft.EntityFrameworkCore;
using dienlanh.Models;

namespace dienlanh.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RepairRequest> RepairRequests { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Component> Components { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RepairRequest>()
                .Property(r => r.FinalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Component>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);
        }
    }
}
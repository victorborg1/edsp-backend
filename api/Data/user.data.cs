using Microsoft.EntityFrameworkCore;
using api.Models;


namespace api.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<Order> Orders { get; set; }

        
        // make the constraints unique.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // make username unique.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Name)
                .IsUnique();
            
            // make the email unique.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // make the license unique.
            modelBuilder.Entity<License>()
            .HasIndex(l => new { l.UserId, l.ProductId })
            .IsUnique();
        }
    }
}
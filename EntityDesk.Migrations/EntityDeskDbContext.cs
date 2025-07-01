using Microsoft.EntityFrameworkCore;
using EntityDesk.Core.Models;

namespace EntityDesk.Migrations
{
    public class EntityDeskDbContext(DbContextOptions<EntityDeskDbContext> options) : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Counterparty> Counterparties { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<Counterparty>().ToTable("Counterparties");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<Employee>().Property(e => e.Position).HasConversion<int>();
            modelBuilder.Entity<Counterparty>()
                .HasOne(c => c.Curator)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Employee)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Counterparty)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 
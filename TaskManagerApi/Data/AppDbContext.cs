
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
        .HasMany(r=>r.Users)
        .WithOne(u=>u.Role)
        .HasForeignKey(u=>u.RoleId)
        .OnDelete(DeleteBehavior.Restrict);

        //Seeding intial roles
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = 1,
                Name = "Admin"
            },
            new Role
            {
                Id = 2,
                Name = "Employee"
            }
        );
    }

}
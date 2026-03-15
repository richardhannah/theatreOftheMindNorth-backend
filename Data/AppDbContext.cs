using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Login> Logins => Set<Login>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Login>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Token);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasOne(e => e.Login)
                  .WithOne()
                  .HasForeignKey<User>(e => e.UserId);
            entity.Property(e => e.Role)
                  .HasConversion<string>();
        });
    }
}

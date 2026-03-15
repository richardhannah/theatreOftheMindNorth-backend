using Microsoft.EntityFrameworkCore;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Login> Logins => Set<Login>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Stash> Stashes => Set<Stash>();

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

        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.CharacterId);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
            entity.Property(e => e.ClassAbilities).HasColumnType("jsonb");
            entity.Property(e => e.Skills).HasColumnType("jsonb");
            entity.Property(e => e.WeaponMasteries).HasColumnType("jsonb");
            entity.Property(e => e.PreparedSpells).HasColumnType("jsonb");
            entity.Property(e => e.Spellbook).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Stash>(entity =>
        {
            entity.HasKey(e => e.StashId);
            entity.HasOne(e => e.Character)
                  .WithMany(c => c.Stashes)
                  .HasForeignKey(e => e.CharacterId)
                  .IsRequired(false);
            entity.Property(e => e.Equipment).HasColumnType("jsonb");
            entity.HasIndex(e => e.Shared);

            // Seed the Expedition Caravan
            entity.HasData(new Stash
            {
                StashId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                CharacterId = null,
                Name = "Expedition Caravan",
                Removable = false,
                Shared = true,
                SortOrder = 1,
                Platinum = 0,
                Gold = 0,
                Electrum = 0,
                Silver = 0,
                Copper = 0,
                Equipment = "[]"
            });
        });
    }
}

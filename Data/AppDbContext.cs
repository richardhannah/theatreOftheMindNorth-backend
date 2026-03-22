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
    public DbSet<VttSceneEntity> VttScenes => Set<VttSceneEntity>();
    public DbSet<PackAnimal> PackAnimals => Set<PackAnimal>();
    public DbSet<Wagon> Wagons => Set<Wagon>();
    public DbSet<ExpeditionState> ExpeditionState => Set<ExpeditionState>();
    public DbSet<Mercenary> Mercenaries => Set<Mercenary>();
    public DbSet<Specialist> Specialists => Set<Specialist>();

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

        modelBuilder.Entity<VttSceneEntity>(entity =>
        {
            entity.HasKey(e => e.SceneId);
            entity.Property(e => e.Counters).HasColumnType("jsonb");
        });

        modelBuilder.Entity<PackAnimal>(entity =>
        {
            entity.HasKey(e => e.PackAnimalId);
            entity.HasOne(e => e.AssignedWagon)
                  .WithMany(w => w.AssignedAnimals)
                  .HasForeignKey(e => e.AssignedWagonId)
                  .IsRequired(false);
        });

        modelBuilder.Entity<Wagon>(entity =>
        {
            entity.HasKey(e => e.WagonId);
        });

        modelBuilder.Entity<ExpeditionState>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisabledRaces).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Mercenary>(entity =>
        {
            entity.HasKey(e => e.MercenaryId);
            entity.HasIndex(e => new { e.Type, e.Race }).IsUnique();
        });

        modelBuilder.Entity<Specialist>(entity =>
        {
            entity.HasKey(e => e.SpecialistId);
            entity.HasIndex(e => e.Type).IsUnique();
        });
    }
}

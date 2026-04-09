using Microsoft.EntityFrameworkCore;
using BankProfiles.Web.Data.Entities;

namespace BankProfiles.Web.Data;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options)
    {
    }

    public DbSet<Bank> Banks { get; set; }
    public DbSet<RatingCriteria> RatingCriterias { get; set; }
    public DbSet<BankRating> BankRatings { get; set; }
    public DbSet<RatingHistory> RatingHistories { get; set; }
    public DbSet<ViewHistory> ViewHistory { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Bank entity configuration
        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.BankId);
            entity.Property(e => e.BankCode)
                .IsRequired()
                .HasMaxLength(50);
            entity.HasIndex(e => e.BankCode)
                .IsUnique();
            entity.HasIndex(e => e.ViewCount);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // RatingCriteria entity configuration
        modelBuilder.Entity<RatingCriteria>(entity =>
        {
            entity.HasKey(e => e.CriteriaId);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.HasIndex(e => e.DisplayOrder);
        });

        // BankRating entity configuration
        modelBuilder.Entity<BankRating>(entity =>
        {
            entity.HasKey(e => e.RatingId);
            entity.Property(e => e.RatingValue)
                .HasPrecision(4, 2);
            entity.Property(e => e.Notes)
                .HasMaxLength(500);
            entity.Property(e => e.RatingDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Bank)
                .WithMany(b => b.BankRatings)
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Criteria)
                .WithMany(c => c.BankRatings)
                .HasForeignKey(e => e.CriteriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.BankId, e.CriteriaId });
        });

        // RatingHistory entity configuration
        modelBuilder.Entity<RatingHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId);
            entity.Property(e => e.OverallRating)
                .HasPrecision(4, 2);
            entity.Property(e => e.RecordedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Bank)
                .WithMany(b => b.RatingHistories)
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Criteria)
                .WithMany(c => c.RatingHistories)
                .HasForeignKey(e => e.CriteriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.BankId, e.CriteriaId, e.RecordedDate });
        });

        // ViewHistory entity configuration
        modelBuilder.Entity<ViewHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId);
            entity.Property(e => e.RecordedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Bank)
                .WithMany(b => b.ViewHistories)
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.BankId, e.RecordedDate });
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed rating criteria
        modelBuilder.Entity<RatingCriteria>().HasData(
            new RatingCriteria { CriteriaId = 1, Name = "Service", DisplayOrder = 1 },
            new RatingCriteria { CriteriaId = 2, Name = "Fees", DisplayOrder = 2 },
            new RatingCriteria { CriteriaId = 3, Name = "Convenience", DisplayOrder = 3 },
            new RatingCriteria { CriteriaId = 4, Name = "Digital Services", DisplayOrder = 4 },
            new RatingCriteria { CriteriaId = 5, Name = "Customer Support", DisplayOrder = 5 }
        );
    }
}

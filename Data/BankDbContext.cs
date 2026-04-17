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
    public DbSet<MetricEvent> MetricEvents { get; set; }
    public DbSet<BankSnapshot> BankSnapshots { get; set; }
    public DbSet<MetricFeedback> MetricFeedbacks { get; set; } = null!;
    public DbSet<FeedbackSubmission> FeedbackSubmissions { get; set; } = null!;

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

        // MetricEvent entity configuration (Event Store)
        modelBuilder.Entity<MetricEvent>(entity =>
        {
            entity.HasKey(e => e.EventId);

            entity.Property(e => e.BankCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Country)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.MetricName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.MetricValue)
                .IsRequired();

            entity.Property(e => e.MetricType)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Comment)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.EventVersion)
                .HasDefaultValue(1);

            entity.HasIndex(e => new { e.BankCode, e.EventSequence })
                .IsUnique();

            entity.HasIndex(e => new { e.BankCode, e.MetricName, e.CreatedDate });

            entity.HasIndex(e => e.BankCode);
        });

        // BankSnapshot entity configuration
        modelBuilder.Entity<BankSnapshot>(entity =>
        {
            entity.HasKey(e => e.SnapshotId);

            entity.Property(e => e.BankCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ProfileJson)
                .IsRequired();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.BankCode, e.EventSequenceUpTo });
        });

        // MetricFeedback entity configuration
        modelBuilder.Entity<MetricFeedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId);
            entity.Property(e => e.MetricCategory)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.MetricName)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(e => e.CurrentValue)
                .HasMaxLength(500);
            entity.Property(e => e.SuggestedValue)
                .HasMaxLength(500);
            entity.Property(e => e.Explanation)
                .IsRequired()
                .HasMaxLength(2000);
            entity.Property(e => e.SubmitterIP)
                .HasMaxLength(45);
            entity.Property(e => e.SubmittedDate)
                .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.ReviewNotes)
                .HasMaxLength(1000);

            entity.HasOne(e => e.Bank)
                .WithMany()
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.BankId);
            entity.HasIndex(e => e.SubmittedDate);
            entity.HasIndex(e => e.Status);
        });

        // FeedbackSubmission entity configuration
        modelBuilder.Entity<FeedbackSubmission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId);
            entity.Property(e => e.SubmitterIP)
                .IsRequired()
                .HasMaxLength(45);
            entity.Property(e => e.SubmissionDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.SubmitterIP);
            entity.HasIndex(e => e.SubmissionDate);
            entity.HasIndex(e => new { e.SubmitterIP, e.SubmissionDate });
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

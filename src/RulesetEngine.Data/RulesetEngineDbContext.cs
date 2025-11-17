// ============================================================
// Data Layer - DbContext
// Location: /src/RulesetEngine.Data/
// ============================================================

using Microsoft.EntityFrameworkCore;
using RulesetEngine.Domain.Model;


namespace RulesetEngine.Data
{
    public class RulesetEngineDbContext : DbContext
    {
        public RulesetEngineDbContext(DbContextOptions<RulesetEngineDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ruleset> Rulesets { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<EvaluationLog> EvaluationLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ruleset Configuration
            modelBuilder.Entity<Ruleset>(entity =>
            {
                entity.HasKey(e => e.RulesetId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.Priority).IsRequired();
                entity.HasIndex(e => new { e.IsActive, e.Priority });
            });

            // Rule Configuration
            modelBuilder.Entity<Rule>(entity =>
            {
                entity.HasKey(e => e.RuleId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ResultProductionPlant).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).IsRequired();
                entity.HasIndex(e => new { e.RulesetId, e.SequenceOrder });

                entity.HasOne(e => e.Ruleset)
                    .WithMany(r => r.Rules)
                    .HasForeignKey(e => e.RulesetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Condition Configuration
            modelBuilder.Entity<Condition>(entity =>
            {
                entity.HasKey(e => e.ConditionId);
                entity.Property(e => e.Field).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Operator).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Value).IsRequired().HasMaxLength(500);

                entity.HasOne(e => e.Ruleset)
                    .WithMany(r => r.Conditions)
                    .HasForeignKey(e => e.RulesetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Rule)
                    .WithMany(r => r.Conditions)
                    .HasForeignKey(e => e.RuleId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // EvaluationLog Configuration
            modelBuilder.Entity<EvaluationLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.OrderId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PublisherNumber).HasMaxLength(50);
                entity.Property(e => e.OrderMethod).HasMaxLength(50);
                entity.Property(e => e.ProductionPlant).HasMaxLength(50);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.EvaluationDate);
            });
        }
    }
}

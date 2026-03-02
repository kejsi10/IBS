using IBS.Policies.Domain.Aggregates.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Policies.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Coverage entity.
/// </summary>
public sealed class CoverageConfiguration : IEntityTypeConfiguration<Coverage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Coverage> builder)
    {
        builder.ToTable("Coverages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PolicyId)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        // LimitAmount value object (Money) - nullable
        builder.OwnsOne(x => x.LimitAmount, la =>
        {
            la.Property(p => p.Amount)
                .HasColumnName("LimitAmount")
                .HasPrecision(18, 2);

            la.Property(p => p.Currency)
                .HasColumnName("LimitCurrency")
                .HasMaxLength(3);
        });

        // PerOccurrenceLimit value object (Money) - nullable
        builder.OwnsOne(x => x.PerOccurrenceLimit, pol =>
        {
            pol.Property(p => p.Amount)
                .HasColumnName("PerOccurrenceLimit")
                .HasPrecision(18, 2);

            pol.Property(p => p.Currency)
                .HasColumnName("PerOccurrenceCurrency")
                .HasMaxLength(3);
        });

        // AggregateLimit value object (Money) - nullable
        builder.OwnsOne(x => x.AggregateLimit, al =>
        {
            al.Property(p => p.Amount)
                .HasColumnName("AggregateLimit")
                .HasPrecision(18, 2);

            al.Property(p => p.Currency)
                .HasColumnName("AggregateCurrency")
                .HasMaxLength(3);
        });

        // DeductibleAmount value object (Money) - nullable
        builder.OwnsOne(x => x.DeductibleAmount, da =>
        {
            da.Property(p => p.Amount)
                .HasColumnName("DeductibleAmount")
                .HasPrecision(18, 2);

            da.Property(p => p.Currency)
                .HasColumnName("DeductibleCurrency")
                .HasMaxLength(3);
        });

        // PremiumAmount value object (Money) - required
        builder.OwnsOne(x => x.PremiumAmount, pa =>
        {
            pa.Property(p => p.Amount)
                .HasColumnName("PremiumAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            pa.Property(p => p.Currency)
                .HasColumnName("PremiumCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(x => x.IsOptional)
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.EffectiveDate);

        // Indexes
        builder.HasIndex(x => x.PolicyId);
        builder.HasIndex(x => new { x.PolicyId, x.Code });
    }
}

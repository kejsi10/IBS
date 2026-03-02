using IBS.Policies.Domain.Aggregates.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Policies.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Endorsement entity.
/// </summary>
public sealed class EndorsementConfiguration : IEntityTypeConfiguration<Endorsement>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Endorsement> builder)
    {
        builder.ToTable("Endorsements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.PolicyId)
            .IsRequired();

        builder.Property(x => x.EndorsementNumber)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EffectiveDate)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000)
            .IsRequired();

        // PremiumChange value object (Money) - required, can be negative
        builder.OwnsOne(x => x.PremiumChange, pc =>
        {
            pc.Property(p => p.Amount)
                .HasColumnName("PremiumChange")
                .HasPrecision(18, 2)
                .IsRequired();

            pc.Property(p => p.Currency)
                .HasColumnName("PremiumChangeCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ProcessedAt);

        builder.Property(x => x.ProcessedBy);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        // Ignore computed properties
        builder.Ignore(x => x.IsAp);
        builder.Ignore(x => x.IsRp);

        // Indexes
        builder.HasIndex(x => x.PolicyId);
        builder.HasIndex(x => new { x.PolicyId, x.EndorsementNumber }).IsUnique();
        builder.HasIndex(x => x.Status);
    }
}

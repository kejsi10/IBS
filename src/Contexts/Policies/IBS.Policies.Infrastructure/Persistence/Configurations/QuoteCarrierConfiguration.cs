using IBS.Policies.Domain.Aggregates.Quote;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Policies.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the QuoteCarrier entity.
/// </summary>
public sealed class QuoteCarrierConfiguration : IEntityTypeConfiguration<QuoteCarrier>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<QuoteCarrier> builder)
    {
        builder.ToTable("QuoteCarriers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.QuoteId)
            .IsRequired();

        builder.Property(x => x.CarrierId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.PremiumAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.PremiumCurrency)
            .HasMaxLength(3);

        builder.Property(x => x.DeclinationReason)
            .HasMaxLength(500);

        builder.Property(x => x.Conditions)
            .HasMaxLength(2000);

        builder.Property(x => x.ProposedCoverages);

        builder.Property(x => x.RespondedAt);

        builder.Property(x => x.ExpiresAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Indexes
        builder.HasIndex(x => x.QuoteId);
        builder.HasIndex(x => new { x.QuoteId, x.CarrierId }).IsUnique();
    }
}

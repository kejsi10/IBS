using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Aggregates.Quote;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Policies.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Quote entity.
/// </summary>
public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.ClientId)
            .IsRequired();

        builder.Property(x => x.LineOfBusiness)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // EffectivePeriod value object
        builder.OwnsOne(x => x.EffectivePeriod, ep =>
        {
            ep.Property(p => p.EffectiveDate)
                .HasColumnName("EffectiveDate")
                .IsRequired();

            ep.Property(p => p.ExpirationDate)
                .HasColumnName("ExpirationDate")
                .IsRequired();
        });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.AcceptedCarrierId);

        builder.Property(x => x.PolicyId);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.IsRenewalQuote)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.RenewalPolicyId);

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Relationships with child entities
        builder.HasMany(x => x.Carriers)
            .WithOne()
            .HasForeignKey(x => x.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);

        // Indexes
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.ClientId });
    }
}

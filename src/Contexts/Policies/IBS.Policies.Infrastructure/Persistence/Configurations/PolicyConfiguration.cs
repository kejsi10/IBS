using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.Policies.Domain.Events;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Policies.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Policy entity.
/// </summary>
public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        // PolicyNumber value object
        builder.OwnsOne(x => x.PolicyNumber, pn =>
        {
            pn.Property(p => p.Value)
                .HasColumnName("PolicyNumber")
                .HasMaxLength(50)
                .IsRequired();

            pn.HasIndex(p => p.Value)
                .IsUnique();
        });

        builder.Property(x => x.ClientId)
            .IsRequired();

        builder.Property(x => x.CarrierId)
            .IsRequired();

        builder.Property(x => x.LineOfBusiness)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PolicyType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
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

        // TotalPremium value object (Money)
        builder.OwnsOne(x => x.TotalPremium, tp =>
        {
            tp.Property(p => p.Amount)
                .HasColumnName("TotalPremium")
                .HasPrecision(18, 2)
                .IsRequired();

            tp.Property(p => p.Currency)
                .HasColumnName("PremiumCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(x => x.BillingType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PaymentPlan)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CarrierPolicyNumber)
            .HasMaxLength(100);

        builder.Property(x => x.QuoteId);

        builder.Property(x => x.PreviousPolicyId);

        builder.Property(x => x.RenewalPolicyId);

        builder.Property(x => x.BoundAt);

        builder.Property(x => x.BoundBy);

        builder.Property(x => x.CancellationDate);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(1000);

        builder.Property(x => x.CancellationType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.ReinstatementDate);

        builder.Property(x => x.ReinstatementReason)
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Relationships with child entities
        builder.HasMany(x => x.Coverages)
            .WithOne()
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Endorsements)
            .WithOne()
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);

        // Indexes
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.ClientId });
        builder.HasIndex(x => new { x.TenantId, x.CarrierId });
        builder.HasIndex(x => new { x.TenantId, x.LineOfBusiness });
    }
}

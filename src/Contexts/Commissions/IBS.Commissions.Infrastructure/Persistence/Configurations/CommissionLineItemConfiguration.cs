using IBS.Commissions.Domain.Aggregates.CommissionStatement;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Commissions.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the CommissionLineItem entity.
/// </summary>
public sealed class CommissionLineItemConfiguration : IEntityTypeConfiguration<CommissionLineItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CommissionLineItem> builder)
    {
        builder.ToTable("CommissionLineItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.StatementId)
            .IsRequired();

        builder.Property(x => x.PolicyId);

        builder.Property(x => x.PolicyNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.InsuredName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.LineOfBusiness)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EffectiveDate)
            .IsRequired();

        builder.Property(x => x.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(x => x.GrossPremium, gp =>
        {
            gp.Property(p => p.Amount)
                .HasColumnName("GrossPremiumAmount")
                .HasPrecision(18, 2);

            gp.Property(p => p.Currency)
                .HasColumnName("GrossPremiumCurrency")
                .HasMaxLength(3);
        });

        builder.Property(x => x.CommissionRate)
            .HasPrecision(8, 4)
            .IsRequired();

        builder.OwnsOne(x => x.CommissionAmount, ca =>
        {
            ca.Property(p => p.Amount)
                .HasColumnName("CommissionAmount")
                .HasPrecision(18, 2);

            ca.Property(p => p.Currency)
                .HasColumnName("CommissionCurrency")
                .HasMaxLength(3);
        });

        builder.Property(x => x.IsReconciled)
            .IsRequired();

        builder.Property(x => x.ReconciledAt);

        builder.Property(x => x.DisputeReason)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.StatementId);
        builder.HasIndex(x => x.PolicyId);
    }
}

using IBS.Commissions.Domain.Aggregates.CommissionStatement;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Commissions.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the CommissionStatement entity.
/// </summary>
public sealed class CommissionStatementConfiguration : IEntityTypeConfiguration<CommissionStatement>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CommissionStatement> builder)
    {
        builder.ToTable("CommissionStatements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.CarrierId)
            .IsRequired();

        builder.Property(x => x.CarrierName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.StatementNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PeriodMonth)
            .IsRequired();

        builder.Property(x => x.PeriodYear)
            .IsRequired();

        builder.Property(x => x.StatementDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(x => x.TotalPremium, tp =>
        {
            tp.Property(p => p.Amount)
                .HasColumnName("TotalPremiumAmount")
                .HasPrecision(18, 2);

            tp.Property(p => p.Currency)
                .HasColumnName("TotalPremiumCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(x => x.TotalCommission, tc =>
        {
            tc.Property(p => p.Amount)
                .HasColumnName("TotalCommissionAmount")
                .HasPrecision(18, 2);

            tc.Property(p => p.Currency)
                .HasColumnName("TotalCommissionCurrency")
                .HasMaxLength(3);
        });

        builder.Property(x => x.ReceivedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasMany(x => x.LineItems)
            .WithOne()
            .HasForeignKey(x => x.StatementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ProducerSplits)
            .WithOne()
            .HasForeignKey(x => x.StatementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.CarrierId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.CarrierId, x.PeriodMonth, x.PeriodYear });
    }
}

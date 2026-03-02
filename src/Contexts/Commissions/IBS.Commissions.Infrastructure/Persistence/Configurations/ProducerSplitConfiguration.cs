using IBS.Commissions.Domain.Aggregates.CommissionStatement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Commissions.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ProducerSplit entity.
/// </summary>
public sealed class ProducerSplitConfiguration : IEntityTypeConfiguration<ProducerSplit>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProducerSplit> builder)
    {
        builder.ToTable("ProducerSplits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.StatementId)
            .IsRequired();

        builder.Property(x => x.LineItemId)
            .IsRequired();

        builder.Property(x => x.ProducerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ProducerId)
            .IsRequired();

        builder.Property(x => x.SplitPercentage)
            .HasPrecision(8, 4)
            .IsRequired();

        builder.OwnsOne(x => x.SplitAmount, sa =>
        {
            sa.Property(p => p.Amount)
                .HasColumnName("SplitAmount")
                .HasPrecision(18, 2);

            sa.Property(p => p.Currency)
                .HasColumnName("SplitCurrency")
                .HasMaxLength(3);
        });

        builder.HasIndex(x => x.StatementId);
        builder.HasIndex(x => x.LineItemId);
        builder.HasIndex(x => x.ProducerId);
    }
}

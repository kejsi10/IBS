using IBS.Claims.Domain.Aggregates.Claim;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Claims.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ClaimPayment entity.
/// </summary>
public sealed class ClaimPaymentConfiguration : IEntityTypeConfiguration<ClaimPayment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClaimPayment> builder)
    {
        builder.ToTable("ClaimPayments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ClaimId)
            .IsRequired();

        builder.Property(x => x.PaymentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(x => x.Amount, a =>
        {
            a.Property(p => p.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            a.Property(p => p.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Property(x => x.PayeeName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PaymentDate)
            .IsRequired();

        builder.Property(x => x.CheckNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.AuthorizedBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AuthorizedAt)
            .IsRequired();

        builder.Property(x => x.IssuedAt);

        builder.Property(x => x.VoidedAt);

        builder.Property(x => x.VoidReason)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.ClaimId);
    }
}

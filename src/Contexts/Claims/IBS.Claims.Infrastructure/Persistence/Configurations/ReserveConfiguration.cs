using IBS.Claims.Domain.Aggregates.Claim;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Claims.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Reserve entity.
/// </summary>
public sealed class ReserveConfiguration : IEntityTypeConfiguration<Reserve>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Reserve> builder)
    {
        builder.ToTable("ClaimReserves");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ClaimId)
            .IsRequired();

        builder.Property(x => x.ReserveType)
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

        builder.Property(x => x.SetBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.SetAt)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.ClaimId);
    }
}

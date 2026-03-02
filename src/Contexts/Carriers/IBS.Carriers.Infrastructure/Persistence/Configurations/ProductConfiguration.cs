using IBS.Carriers.Domain.Aggregates.Carrier;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Carriers.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Product entity.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.CarrierId)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.LineOfBusiness)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.MinimumPremium)
            .HasPrecision(12, 2);

        builder.Property(p => p.EffectiveDate);

        builder.Property(p => p.ExpirationDate);

        // Audit columns
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt);

        // Index for carrier + code uniqueness
        builder.HasIndex(p => new { p.CarrierId, p.Code })
            .IsUnique();
    }
}

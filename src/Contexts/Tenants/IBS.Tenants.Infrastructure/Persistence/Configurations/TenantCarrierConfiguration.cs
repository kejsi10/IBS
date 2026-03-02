using IBS.Tenants.Domain.Aggregates.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Tenants.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the TenantCarrier entity.
/// </summary>
public sealed class TenantCarrierConfiguration : IEntityTypeConfiguration<TenantCarrier>
{
    public void Configure(EntityTypeBuilder<TenantCarrier> builder)
    {
        builder.ToTable("TenantCarriers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.CarrierId)
            .IsRequired();

        builder.Property(x => x.AgencyCode)
            .HasMaxLength(50);

        builder.Property(x => x.CommissionRate)
            .HasPrecision(5, 4);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.CarrierId }).IsUnique();
    }
}

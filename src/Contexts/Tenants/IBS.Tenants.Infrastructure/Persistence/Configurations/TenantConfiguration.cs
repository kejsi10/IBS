using IBS.Tenants.Domain.Aggregates.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Tenants.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Tenant entity.
/// </summary>
public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        // Subdomain value object
        builder.OwnsOne(x => x.Subdomain, sd =>
        {
            sd.Property(p => p.Value)
                .HasColumnName("Subdomain")
                .HasMaxLength(100)
                .IsRequired();

            sd.HasIndex(p => p.Value).IsUnique();
        });

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SubscriptionTier)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.DefaultCurrency)
            .HasMaxLength(3)
            .HasDefaultValue("USD")
            .IsRequired();

        builder.Property(x => x.Settings)
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(x => x.Carriers)
            .WithOne()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);

        // Indexes
        builder.HasIndex(x => x.Status);
    }
}

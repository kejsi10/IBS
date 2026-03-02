using IBS.Carriers.Domain.Aggregates.Carrier;
using IBS.Carriers.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Carriers.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Carrier entity.
/// </summary>
public sealed class CarrierConfiguration : IEntityTypeConfiguration<Carrier>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Carrier> builder)
    {
        builder.ToTable("Carriers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.OwnsOne(c => c.Code, code =>
        {
            code.Property(c => c.Value)
                .HasColumnName("Code")
                .IsRequired()
                .HasMaxLength(10);

            code.HasIndex(c => c.Value)
                .IsUnique();
        });

        builder.Property(c => c.LegalName)
            .HasMaxLength(500);

        builder.OwnsOne(c => c.AmBestRating, rating =>
        {
            rating.Property(r => r.Value)
                .HasColumnName("AmBestRating")
                .HasMaxLength(10);
        });

        builder.Property(c => c.NaicCode)
            .HasMaxLength(5);

        builder.Property(c => c.WebsiteUrl)
            .HasMaxLength(500);

        builder.Property(c => c.ApiEndpoint)
            .HasMaxLength(500);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        // Configure navigation to Products
        builder.HasMany(c => c.Products)
            .WithOne()
            .HasForeignKey(p => p.CarrierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure navigation to Appetites
        builder.HasMany(c => c.Appetites)
            .WithOne()
            .HasForeignKey(a => a.CarrierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Audit columns
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

    }
}

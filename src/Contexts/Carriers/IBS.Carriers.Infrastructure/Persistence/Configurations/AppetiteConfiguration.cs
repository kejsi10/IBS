using IBS.Carriers.Domain.Aggregates.Carrier;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Carriers.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Appetite entity.
/// </summary>
public sealed class AppetiteConfiguration : IEntityTypeConfiguration<Appetite>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Appetite> builder)
    {
        builder.ToTable("Appetites");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CarrierId)
            .IsRequired();

        builder.Property(a => a.LineOfBusiness)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.States)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.MinYearsInBusiness);

        builder.Property(a => a.MaxYearsInBusiness);

        builder.Property(a => a.MinAnnualRevenue)
            .HasPrecision(18, 2);

        builder.Property(a => a.MaxAnnualRevenue)
            .HasPrecision(18, 2);

        builder.Property(a => a.MinEmployees);

        builder.Property(a => a.MaxEmployees);

        builder.Property(a => a.AcceptedIndustries)
            .HasMaxLength(1000);

        builder.Property(a => a.ExcludedIndustries)
            .HasMaxLength(1000);

        builder.Property(a => a.Notes)
            .HasMaxLength(2000);

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit columns
        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt);

        // Index for querying by carrier and line of business
        builder.HasIndex(a => new { a.CarrierId, a.LineOfBusiness });
    }
}

using IBS.Commissions.Domain.Aggregates.CommissionSchedule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Commissions.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the CommissionSchedule entity.
/// </summary>
public sealed class CommissionScheduleConfiguration : IEntityTypeConfiguration<CommissionSchedule>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CommissionSchedule> builder)
    {
        builder.ToTable("CommissionSchedules");

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

        builder.Property(x => x.LineOfBusiness)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NewBusinessRate)
            .HasPrecision(8, 4)
            .IsRequired();

        builder.Property(x => x.RenewalRate)
            .HasPrecision(8, 4)
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        builder.Property(x => x.EffectiveTo);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.CarrierId, x.LineOfBusiness });
        builder.HasIndex(x => new { x.TenantId, x.IsActive });
    }
}

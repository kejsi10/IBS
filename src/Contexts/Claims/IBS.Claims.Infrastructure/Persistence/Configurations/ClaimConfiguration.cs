using IBS.Claims.Domain.Aggregates.Claim;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Claims.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Claim entity.
/// </summary>
public sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.OwnsOne(x => x.ClaimNumber, cn =>
        {
            cn.Property(p => p.Value)
                .HasColumnName("ClaimNumber")
                .HasMaxLength(20)
                .IsRequired();

            cn.HasIndex(p => p.Value)
                .IsUnique();
        });

        builder.Property(x => x.PolicyId)
            .IsRequired();

        builder.Property(x => x.ClientId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.LossDate)
            .IsRequired();

        builder.Property(x => x.ReportedDate)
            .IsRequired();

        builder.Property(x => x.LossType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.LossDescription)
            .HasMaxLength(4000)
            .IsRequired();

        builder.OwnsOne(x => x.LossAmount, la =>
        {
            la.Property(p => p.Amount)
                .HasColumnName("LossAmount")
                .HasPrecision(18, 2);

            la.Property(p => p.Currency)
                .HasColumnName("LossCurrency")
                .HasMaxLength(3);
        });

        builder.OwnsOne(x => x.ClaimAmount, ca =>
        {
            ca.Property(p => p.Amount)
                .HasColumnName("ClaimAmount")
                .HasPrecision(18, 2);

            ca.Property(p => p.Currency)
                .HasColumnName("ClaimCurrency")
                .HasMaxLength(3);
        });

        builder.Property(x => x.AssignedAdjusterId)
            .HasMaxLength(100);

        builder.Property(x => x.DenialReason)
            .HasMaxLength(2000);

        builder.Property(x => x.ClosedAt);

        builder.Property(x => x.ClosureReason)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedBy)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasMany(x => x.Notes)
            .WithOne()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reserves)
            .WithOne()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Payments)
            .WithOne()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.PolicyId });
        builder.HasIndex(x => new { x.TenantId, x.ClientId });
    }
}

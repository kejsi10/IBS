using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the AuditLog entity.
/// </summary>
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId);

        builder.Property(a => a.UserId);

        builder.Property(a => a.UserEmail)
            .HasMaxLength(256);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Changes)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}

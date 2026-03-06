using IBS.Policies.Domain.Aggregates.Policy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Policies.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the PolicyHistory entity.
/// </summary>
public sealed class PolicyHistoryConfiguration : IEntityTypeConfiguration<PolicyHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PolicyHistory> builder)
    {
        builder.ToTable("PolicyHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.PolicyId)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ChangesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.UserId);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Ignore RowVersion and UpdatedAt — not needed for immutable history entries
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.UpdatedAt);

        builder.HasIndex(x => new { x.TenantId, x.PolicyId });
    }
}

using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.PolicyAssistant.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ReferenceDocument"/> aggregate.
/// </summary>
public sealed class ReferenceDocumentConfiguration : IEntityTypeConfiguration<ReferenceDocument>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ReferenceDocument> builder)
    {
        builder.ToTable("PolicyAssistantReferenceDocuments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.LineOfBusiness)
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .HasMaxLength(2);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Source)
            .HasMaxLength(500);

        builder.HasMany(x => x.Chunks)
            .WithOne()
            .HasForeignKey(c => c.ReferenceDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        // Ignore TenantId for global query filter — ReferenceDocument is not a TenantAggregateRoot
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => new { x.LineOfBusiness, x.State });
    }
}

using IBS.Documents.Domain.Aggregates.Document;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Documents.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Document entity.
/// </summary>
public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.TenantId).IsRequired();
        builder.Property(d => d.EntityType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(d => d.EntityId);
        builder.Property(d => d.FileName).HasMaxLength(260).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(d => d.FileSizeBytes).IsRequired();
        builder.Property(d => d.BlobKey).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(d => d.Version).IsRequired();
        builder.Property(d => d.IsArchived).IsRequired();
        builder.Property(d => d.UploadedBy).HasMaxLength(256).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(1000);
        builder.Property(d => d.UploadedAt).IsRequired();

        builder.HasIndex(d => d.TenantId);
        builder.HasIndex(d => new { d.TenantId, d.EntityType, d.EntityId });
    }
}

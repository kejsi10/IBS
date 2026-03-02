using IBS.PolicyAssistant.Domain.Aggregates.ReferenceDocument;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.PolicyAssistant.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="DocumentChunk"/> child entity.
/// </summary>
public sealed class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("PolicyAssistantDocumentChunks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChunkIndex).IsRequired();

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.ReferenceDocumentId);
        builder.HasIndex(x => new { x.ReferenceDocumentId, x.ChunkIndex });
    }
}

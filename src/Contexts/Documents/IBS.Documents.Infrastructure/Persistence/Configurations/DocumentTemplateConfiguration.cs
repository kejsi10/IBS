using IBS.Documents.Domain.Aggregates.DocumentTemplate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Documents.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the DocumentTemplate entity.
/// </summary>
public sealed class DocumentTemplateConfiguration : IEntityTypeConfiguration<DocumentTemplate>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<DocumentTemplate> builder)
    {
        builder.ToTable("DocumentTemplates");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.TenantId).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(1000).IsRequired();
        builder.Property(t => t.TemplateType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(t => t.Content).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(t => t.IsActive).IsRequired();
        builder.Property(t => t.Version).IsRequired();
        builder.Property(t => t.CreatedBy).HasMaxLength(256).IsRequired();

        builder.HasIndex(t => t.TenantId);
        builder.HasIndex(t => new { t.TenantId, t.TemplateType, t.IsActive });
    }
}

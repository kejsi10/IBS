using IBS.Claims.Domain.Aggregates.Claim;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.Claims.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ClaimNote entity.
/// </summary>
public sealed class ClaimNoteConfiguration : IEntityTypeConfiguration<ClaimNote>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClaimNote> builder)
    {
        builder.ToTable("ClaimNotes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ClaimId)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.AuthorBy)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsInternal)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.HasIndex(x => x.ClaimId);
    }
}

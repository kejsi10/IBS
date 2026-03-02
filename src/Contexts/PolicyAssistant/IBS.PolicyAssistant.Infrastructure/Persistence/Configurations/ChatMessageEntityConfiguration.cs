using IBS.PolicyAssistant.Domain.Aggregates.Conversation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IBS.PolicyAssistant.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ChatMessageEntity"/> child entity.
/// </summary>
public sealed class ChatMessageEntityConfiguration : IEntityTypeConfiguration<ChatMessageEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ChatMessageEntity> builder)
    {
        builder.ToTable("PolicyAssistantMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.MessageType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.ConversationId);
    }
}

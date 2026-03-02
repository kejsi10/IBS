using IBS.BuildingBlocks.Infrastructure.Persistence;
using IBS.PolicyAssistant.Domain.Aggregates.Conversation;
using IBS.PolicyAssistant.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.PolicyAssistant.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for the <see cref="Conversation"/> aggregate.
/// </summary>
public sealed class ConversationRepository(DbContext context)
    : Repository<Conversation, DbContext>(context), IConversationRepository
{
    /// <inheritdoc />
    public override async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}

using IBS.PolicyAssistant.Domain.Aggregates.Conversation;
using IBS.PolicyAssistant.Domain.Queries;
using Microsoft.EntityFrameworkCore;

namespace IBS.PolicyAssistant.Infrastructure.Persistence;

/// <summary>
/// Query implementation for reading <see cref="Conversation"/> data (no-tracking reads).
/// </summary>
public sealed class ConversationQueries(DbContext context) : IConversationQueries
{
    /// <inheritdoc />
    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Set<Conversation>()
            .AsNoTracking()
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Conversation>> GetByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Set<Conversation>()
            .AsNoTracking()
            .Include(c => c.Messages)
            .Where(c => c.TenantId == tenantId && c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken);
    }
}

using IBS.PolicyAssistant.Domain.Aggregates.Conversation;

namespace IBS.PolicyAssistant.Domain.Queries;

/// <summary>
/// Query interface for reading <see cref="Conversation"/> data.
/// </summary>
public interface IConversationQueries
{
    /// <summary>
    /// Gets a conversation by its identifier, including all messages.
    /// </summary>
    /// <param name="id">The conversation identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The conversation, or null if not found.</returns>
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active conversations for a specific user within a tenant, ordered by most recent first.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of conversations.</returns>
    Task<IReadOnlyList<Conversation>> GetByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

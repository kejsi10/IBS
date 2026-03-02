using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Domain.Aggregates.Conversation;

namespace IBS.PolicyAssistant.Domain.Repositories;

/// <summary>
/// Repository interface for the <see cref="Conversation"/> aggregate root.
/// </summary>
public interface IConversationRepository : IRepository<Conversation>
{
}

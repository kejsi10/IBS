using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Domain.Repositories;

namespace IBS.PolicyAssistant.Application.Commands.AbandonConversation;

/// <summary>
/// Handler for the <see cref="AbandonConversationCommand"/>.
/// </summary>
public sealed class AbandonConversationCommandHandler(
    IConversationRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<AbandonConversationCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(AbandonConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await repository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
            return Error.NotFound("Conversation", request.ConversationId);

        if (conversation.TenantId != request.TenantId || conversation.UserId != request.UserId)
            return Error.Forbidden();

        conversation.Abandon();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.PolicyAssistant.Domain.Aggregates.Conversation;
using IBS.PolicyAssistant.Domain.Repositories;

namespace IBS.PolicyAssistant.Application.Commands.CreateConversation;

/// <summary>
/// Handler for the <see cref="CreateConversationCommand"/>.
/// </summary>
public sealed class CreateConversationCommandHandler(
    IConversationRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateConversationCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = Conversation.Create(
            request.TenantId,
            request.UserId,
            request.Title,
            request.Mode,
            request.LineOfBusiness);

        await repository.AddAsync(conversation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return conversation.Id;
    }
}

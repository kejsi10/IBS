using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Repositories;

namespace IBS.Clients.Application.Commands.RemoveContact;

/// <summary>
/// Handler for RemoveContactCommand.
/// </summary>
public sealed class RemoveContactCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveContactCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(RemoveContactCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithChildrenAsync(request.ClientId, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Client", request.ClientId);
        }

        try
        {
            client.RemoveContact(request.ContactId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}

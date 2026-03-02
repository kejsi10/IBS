using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Repositories;

namespace IBS.Clients.Application.Commands.LogCommunication;

/// <summary>
/// Handler for LogCommunicationCommand.
/// </summary>
public sealed class LogCommunicationCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<LogCommunicationCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(LogCommunicationCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithChildrenAsync(request.ClientId, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Client", request.ClientId);
        }

        var communication = client.LogCommunication(
            request.CommunicationType,
            request.Subject,
            request.Notes,
            request.UserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return communication.Id;
    }
}

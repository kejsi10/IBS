using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Repositories;

namespace IBS.Clients.Application.Commands.DeactivateClient;

/// <summary>
/// Handler for the DeactivateClientCommand.
/// </summary>
public sealed class DeactivateClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeactivateClientCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(DeactivateClientCommand request, CancellationToken cancellationToken)
    {
        // Get the client
        var client = await clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
        {
            return Error.NotFound($"Client {request.ClientId} not found.");
        }

        // Deactivate the client
        client.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

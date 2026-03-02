using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Repositories;

namespace IBS.Clients.Application.Commands.RemoveAddress;

/// <summary>
/// Handler for RemoveAddressCommand.
/// </summary>
public sealed class RemoveAddressCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveAddressCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(RemoveAddressCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithChildrenAsync(request.ClientId, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Client", request.ClientId);
        }

        try
        {
            client.RemoveAddress(request.AddressId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}

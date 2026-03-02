using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Repositories;

namespace IBS.Clients.Application.Commands.SetPrimaryAddress;

/// <summary>
/// Handler for SetPrimaryAddressCommand.
/// </summary>
public sealed class SetPrimaryAddressCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SetPrimaryAddressCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(SetPrimaryAddressCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithChildrenAsync(request.ClientId, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Client", request.ClientId);
        }

        try
        {
            client.SetPrimaryAddress(request.AddressId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}

using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Repositories;

namespace IBS.Clients.Application.Commands.AddAddress;

/// <summary>
/// Handler for AddAddressCommand.
/// </summary>
public sealed class AddAddressCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddAddressCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithChildrenAsync(request.ClientId, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Client", request.ClientId);
        }

        var address = client.AddAddress(
            request.AddressType,
            request.StreetLine1,
            request.City,
            request.State,
            request.PostalCode,
            request.StreetLine2,
            request.Country,
            request.IsPrimary);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return address.Id;
    }
}

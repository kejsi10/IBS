using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Repositories;

namespace IBS.Clients.Application.Commands.UpdateAddress;

/// <summary>
/// Handler for UpdateAddressCommand.
/// </summary>
public sealed class UpdateAddressCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateAddressCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithChildrenAsync(request.ClientId, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Client", request.ClientId);
        }

        var address = client.Addresses.FirstOrDefault(a => a.Id == request.AddressId);
        if (address is null)
        {
            return Error.NotFound("Address", request.AddressId);
        }

        address.Update(
            request.StreetLine1,
            request.StreetLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

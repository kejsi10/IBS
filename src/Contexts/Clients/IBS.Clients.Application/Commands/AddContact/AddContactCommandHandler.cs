using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Repositories;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Application.Commands.AddContact;

/// <summary>
/// Handler for AddContactCommand.
/// </summary>
public sealed class AddContactCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddContactCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddContactCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithChildrenAsync(request.ClientId, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Client", request.ClientId);
        }

        // Create the person name value object
        var personName = PersonName.Create(
            request.FirstName,
            request.LastName,
            request.MiddleName,
            request.Suffix);

        // Create optional value objects
        EmailAddress? email = null;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            email = EmailAddress.Create(request.Email);
        }

        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            phone = PhoneNumber.Create(request.Phone);
        }

        // Add the contact
        var contact = client.AddContact(
            personName,
            request.Title,
            email,
            phone,
            request.IsPrimary);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return contact.Id;
    }
}

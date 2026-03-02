using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Domain.Repositories;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Application.Commands.UpdateContact;

/// <summary>
/// Handler for UpdateContactCommand.
/// </summary>
public sealed class UpdateContactCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateContactCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdWithChildrenAsync(request.ClientId, cancellationToken);

        if (client is null)
        {
            return Error.NotFound("Client", request.ClientId);
        }

        var contact = client.Contacts.FirstOrDefault(c => c.Id == request.ContactId);
        if (contact is null)
        {
            return Error.NotFound("Contact", request.ContactId);
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

        // Update the contact
        contact.Update(personName, request.Title, email, phone);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

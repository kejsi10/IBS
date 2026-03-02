using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Application.Queries;
using IBS.Clients.Domain.Aggregates.Client;
using IBS.Clients.Domain.Repositories;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Application.Commands.CreateIndividualClient;

/// <summary>
/// Handler for the CreateIndividualClientCommand.
/// </summary>
public sealed class CreateIndividualClientCommandHandler(
    IClientRepository clientRepository,
    IClientQueries clientQueries,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateIndividualClientCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateIndividualClientCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await clientQueries.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
            {
                return Error.Conflict($"A client with email '{request.Email}' already exists.");
            }
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

        // Create the client
        var client = Client.CreateIndividual(
            request.TenantId,
            personName,
            request.UserId,
            request.DateOfBirth,
            email,
            phone);

        // Persist the client
        await clientRepository.AddAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return client.Id;
    }
}

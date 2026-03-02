using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Application.Queries;
using IBS.Clients.Domain.Aggregates.Client;
using IBS.Clients.Domain.Repositories;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Application.Commands.CreateBusinessClient;

/// <summary>
/// Handler for the CreateBusinessClientCommand.
/// </summary>
public sealed class CreateBusinessClientCommandHandler(
    IClientRepository clientRepository,
    IClientQueries clientQueries,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateBusinessClientCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateBusinessClientCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await clientQueries.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
            {
                return Error.Conflict($"A client with email '{request.Email}' already exists.");
            }
        }

        // Create the business info value object
        var businessInfo = BusinessInfo.Create(
            request.BusinessName,
            request.BusinessType,
            request.DbaName,
            request.Industry,
            request.YearEstablished,
            request.NumberOfEmployees,
            request.AnnualRevenue,
            request.Website);

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
        var client = Client.CreateBusiness(
            request.TenantId,
            businessInfo,
            request.UserId,
            email,
            phone);

        // Persist the client
        await clientRepository.AddAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return client.Id;
    }
}

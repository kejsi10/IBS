using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Clients.Application.Queries;
using IBS.Clients.Domain.Repositories;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Application.Commands.UpdateClient;

/// <summary>
/// Handler for the UpdateClientCommand.
/// </summary>
public sealed class UpdateClientCommandHandler(
    IClientRepository clientRepository,
    IClientQueries clientQueries,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateClientCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        // Get the client
        var client = await clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client is null)
        {
            return Error.NotFound($"Client {request.ClientId} not found.");
        }

        // Check if email already exists (if being updated)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await clientQueries.EmailExistsAsync(request.Email, request.ClientId, cancellationToken))
            {
                return Error.Conflict($"A client with email '{request.Email}' already exists.");
            }
        }

        // Update based on client type
        if (client.ClientType == ClientType.Individual)
        {
            if (!string.IsNullOrWhiteSpace(request.FirstName) && !string.IsNullOrWhiteSpace(request.LastName))
            {
                var personName = PersonName.Create(
                    request.FirstName,
                    request.LastName,
                    request.MiddleName,
                    request.Suffix);
                client.UpdatePersonName(personName);
            }
        }
        else if (client.ClientType == ClientType.Business)
        {
            if (!string.IsNullOrWhiteSpace(request.BusinessName) && !string.IsNullOrWhiteSpace(request.BusinessType))
            {
                var businessInfo = BusinessInfo.Create(
                    request.BusinessName,
                    request.BusinessType,
                    request.DbaName,
                    request.Industry,
                    request.YearEstablished,
                    request.NumberOfEmployees,
                    request.AnnualRevenue,
                    request.Website);
                client.UpdateBusinessInfo(businessInfo);
            }
        }

        // Update email if provided
        if (request.Email is not null)
        {
            EmailAddress? email = null;
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                email = EmailAddress.Create(request.Email);
            }
            client.UpdateEmail(email);
        }

        // Update phone if provided
        if (request.Phone is not null)
        {
            PhoneNumber? phone = null;
            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                phone = PhoneNumber.Create(request.Phone);
            }
            client.UpdatePhone(phone);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

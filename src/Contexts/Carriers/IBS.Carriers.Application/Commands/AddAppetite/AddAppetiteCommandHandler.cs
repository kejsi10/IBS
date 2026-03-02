using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.Repositories;
using IBS.Carriers.Domain.ValueObjects;

namespace IBS.Carriers.Application.Commands.AddAppetite;

/// <summary>
/// Handler for the AddAppetiteCommand.
/// </summary>
public sealed class AddAppetiteCommandHandler(
    ICarrierRepository carrierRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddAppetiteCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddAppetiteCommand request, CancellationToken cancellationToken)
    {
        // Get the carrier
        var carrier = await carrierRepository.GetByIdAsync(request.CarrierId, cancellationToken);
        if (carrier is null)
        {
            return Error.NotFound("Carrier", request.CarrierId);
        }

        ConcurrencyGuard.Validate(request.ExpectedRowVersion, carrier.RowVersion);

        // Check if appetite for this line of business already exists
        var existingAppetite = carrier.GetAppetiteForLineOfBusiness(request.LineOfBusiness);
        if (existingAppetite is not null)
        {
            return Error.Conflict($"An active appetite rule for {request.LineOfBusiness.GetDisplayName()} already exists for this carrier.");
        }

        // Add the appetite
        var appetite = carrier.AddAppetite(request.LineOfBusiness, request.States);

        // Set optional properties
        if (request.MinYearsInBusiness.HasValue || request.MaxYearsInBusiness.HasValue)
        {
            appetite.SetYearsInBusinessRequirement(request.MinYearsInBusiness, request.MaxYearsInBusiness);
        }

        if (request.MinAnnualRevenue.HasValue || request.MaxAnnualRevenue.HasValue)
        {
            appetite.SetRevenueRequirement(request.MinAnnualRevenue, request.MaxAnnualRevenue);
        }

        if (request.MinEmployees.HasValue || request.MaxEmployees.HasValue)
        {
            appetite.SetEmployeeRequirement(request.MinEmployees, request.MaxEmployees);
        }

        if (!string.IsNullOrWhiteSpace(request.AcceptedIndustries) || !string.IsNullOrWhiteSpace(request.ExcludedIndustries))
        {
            appetite.SetIndustryRestrictions(request.AcceptedIndustries, request.ExcludedIndustries);
        }

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            appetite.SetNotes(request.Notes);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return appetite.Id;
    }
}

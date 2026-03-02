using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Domain.Repositories;

namespace IBS.Tenants.Application.Commands.AddTenantCarrier;

/// <summary>
/// Handler for AddTenantCarrierCommand.
/// </summary>
public sealed class AddTenantCarrierCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddTenantCarrierCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(AddTenantCarrierCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdWithCarriersAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Error.NotFound("Tenant", request.TenantId);
        }

        try
        {
            tenant.AddCarrier(request.CarrierId, request.AgencyCode, request.CommissionRate);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}

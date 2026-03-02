using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Domain.Repositories;

namespace IBS.Tenants.Application.Commands.RemoveTenantCarrier;

/// <summary>
/// Handler for RemoveTenantCarrierCommand.
/// </summary>
public sealed class RemoveTenantCarrierCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveTenantCarrierCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(RemoveTenantCarrierCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdWithCarriersAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Error.NotFound("Tenant", request.TenantId);
        }

        try
        {
            tenant.RemoveCarrier(request.CarrierId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}

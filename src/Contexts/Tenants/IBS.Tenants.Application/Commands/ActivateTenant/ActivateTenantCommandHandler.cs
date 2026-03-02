using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Domain.Repositories;

namespace IBS.Tenants.Application.Commands.ActivateTenant;

/// <summary>
/// Handler for ActivateTenantCommand.
/// </summary>
public sealed class ActivateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ActivateTenantCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Error.NotFound("Tenant", request.TenantId);
        }

        try
        {
            tenant.Activate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Error.Validation(ex.Message);
        }
    }
}

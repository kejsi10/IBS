using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Domain.Repositories;

namespace IBS.Tenants.Application.Commands.CancelTenant;

/// <summary>
/// Handler for CancelTenantCommand.
/// </summary>
public sealed class CancelTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CancelTenantCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(CancelTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Error.NotFound("Tenant", request.TenantId);
        }

        tenant.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

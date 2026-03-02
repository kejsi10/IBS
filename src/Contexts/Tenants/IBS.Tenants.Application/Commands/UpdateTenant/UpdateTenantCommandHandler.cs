using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Domain.Repositories;

namespace IBS.Tenants.Application.Commands.UpdateTenant;

/// <summary>
/// Handler for UpdateTenantCommand.
/// </summary>
public sealed class UpdateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateTenantCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Error.NotFound("Tenant", request.TenantId);
        }

        tenant.UpdateName(request.Name);
        tenant.UpdateSettings(request.Settings);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Domain.Repositories;

namespace IBS.Tenants.Application.Commands.UpdateSubscriptionTier;

/// <summary>
/// Handler for UpdateSubscriptionTierCommand.
/// </summary>
public sealed class UpdateSubscriptionTierCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateSubscriptionTierCommand>
{
    /// <inheritdoc />
    public async Task<Result> Handle(UpdateSubscriptionTierCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Error.NotFound("Tenant", request.TenantId);
        }

        tenant.UpdateSubscriptionTier(request.SubscriptionTier);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Application.Queries;
using IBS.Tenants.Domain.Aggregates.Tenant;
using IBS.Tenants.Domain.Repositories;
using IBS.Tenants.Domain.ValueObjects;

namespace IBS.Tenants.Application.Commands.CreateTenant;

/// <summary>
/// Handler for CreateTenantCommand.
/// </summary>
public sealed class CreateTenantCommandHandler(
    ITenantRepository tenantRepository,
    ITenantQueries tenantQueries,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTenantCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // Check if subdomain already exists
        var subdomainExists = await tenantQueries.SubdomainExistsAsync(
            request.Subdomain,
            cancellationToken: cancellationToken);

        if (subdomainExists)
        {
            return Error.Conflict($"Subdomain '{request.Subdomain}' is already in use.");
        }

        // Create subdomain value object
        var subdomain = Subdomain.Create(request.Subdomain);

        // Create tenant
        var tenant = Tenant.Create(request.Name, subdomain, request.SubscriptionTier);

        // Persist
        await tenantRepository.AddAsync(tenant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(tenant.Id);
    }
}

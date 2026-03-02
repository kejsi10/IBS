using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Tenants.Application.Queries.GetTenantById;

/// <summary>
/// Query to get a tenant by identifier.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record GetTenantByIdQuery(Guid TenantId) : IQuery<TenantDetailsDto?>;

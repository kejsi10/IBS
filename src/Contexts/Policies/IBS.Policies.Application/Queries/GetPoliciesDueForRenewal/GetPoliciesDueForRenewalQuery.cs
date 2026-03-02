using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Application.Queries.GetPolicies;

namespace IBS.Policies.Application.Queries.GetPoliciesDueForRenewal;

/// <summary>
/// Query to get policies due for renewal (within N days of expiration).
/// </summary>
public sealed record GetPoliciesDueForRenewalQuery(
    Guid TenantId,
    int DaysUntilExpiration = 60,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<PolicyListResult>;

using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Application.Queries.GetPolicies;

namespace IBS.Policies.Application.Queries.GetExpiringPolicies;

/// <summary>
/// Query to get policies expiring within a date range.
/// </summary>
public sealed record GetExpiringPoliciesQuery(
    Guid TenantId,
    DateOnly StartDate,
    DateOnly EndDate,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<PolicyListResult>;

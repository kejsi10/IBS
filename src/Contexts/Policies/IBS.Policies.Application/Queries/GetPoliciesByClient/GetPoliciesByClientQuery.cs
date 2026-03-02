using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Application.Queries.GetPolicies;

namespace IBS.Policies.Application.Queries.GetPoliciesByClient;

/// <summary>
/// Query to get all policies for a specific client.
/// </summary>
public sealed record GetPoliciesByClientQuery(
    Guid TenantId,
    Guid ClientId,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    string SortDirection = "desc"
) : IQuery<PolicyListResult>;

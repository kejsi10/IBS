using IBS.BuildingBlocks.Application.Queries;
using IBS.Carriers.Domain.ValueObjects;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Application.Queries.GetPolicies;

/// <summary>
/// Query to search and list policies.
/// </summary>
public sealed record GetPoliciesQuery(
    Guid TenantId,
    string? SearchTerm = null,
    Guid? ClientId = null,
    Guid? CarrierId = null,
    PolicyStatus? Status = null,
    LineOfBusiness? LineOfBusiness = null,
    DateOnly? EffectiveDateFrom = null,
    DateOnly? EffectiveDateTo = null,
    DateOnly? ExpirationDateFrom = null,
    DateOnly? ExpirationDateTo = null,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    string SortDirection = "desc"
) : IQuery<PolicyListResult>;

/// <summary>
/// Result for policy list query.
/// </summary>
public sealed record PolicyListResult(
    IReadOnlyList<PolicyListItemDto> Policies,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

/// <summary>
/// DTO for policy list items.
/// </summary>
public sealed record PolicyListItemDto(
    Guid Id,
    string PolicyNumber,
    Guid ClientId,
    string? ClientName,
    Guid CarrierId,
    string? CarrierName,
    string LineOfBusiness,
    string PolicyType,
    string Status,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    decimal TotalPremium,
    string Currency,
    DateTimeOffset CreatedAt
);

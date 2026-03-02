using IBS.BuildingBlocks.Application.Queries;
using IBS.Claims.Application.DTOs;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Application.Queries.GetClaims;

/// <summary>
/// Query to search and list claims.
/// </summary>
public sealed record GetClaimsQuery(
    Guid TenantId,
    string? SearchTerm = null,
    ClaimStatus? Status = null,
    Guid? PolicyId = null,
    Guid? ClientId = null,
    LossType? LossType = null,
    DateTimeOffset? LossDateFrom = null,
    DateTimeOffset? LossDateTo = null,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    string SortDirection = "desc"
) : IQuery<ClaimListResult>;

/// <summary>
/// Result for claim list query.
/// </summary>
public sealed record ClaimListResult(
    IReadOnlyList<ClaimListItemDto> Claims,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

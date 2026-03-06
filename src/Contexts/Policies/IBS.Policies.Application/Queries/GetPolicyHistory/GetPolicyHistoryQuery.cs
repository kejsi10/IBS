using IBS.BuildingBlocks.Application.Queries;
using IBS.Policies.Domain.Aggregates.Policy;

namespace IBS.Policies.Application.Queries.GetPolicyHistory;

/// <summary>
/// Query to retrieve paginated history entries for a policy.
/// </summary>
public sealed record GetPolicyHistoryQuery(
    Guid TenantId,
    Guid PolicyId,
    int Page = 1,
    int PageSize = 50
) : IQuery<PolicyHistoryResult>;

/// <summary>
/// Paginated result for policy history query.
/// </summary>
public sealed record PolicyHistoryResult(
    IReadOnlyList<PolicyHistoryItemDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

/// <summary>
/// DTO for a single policy history entry.
/// </summary>
public sealed record PolicyHistoryItemDto(
    Guid Id,
    PolicyHistoryEventType EventType,
    string Description,
    string? ChangesJson,
    Guid? UserId,
    DateTimeOffset Timestamp
);

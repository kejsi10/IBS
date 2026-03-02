using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;

namespace IBS.Commissions.Application.Queries.GetSchedules;

/// <summary>
/// Query to search and list commission schedules.
/// </summary>
public sealed record GetSchedulesQuery(
    Guid TenantId,
    string? SearchTerm = null,
    Guid? CarrierId = null,
    string? LineOfBusiness = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    string SortDirection = "desc"
) : IQuery<ScheduleListResult>;

/// <summary>
/// Result for schedule list query.
/// </summary>
public sealed record ScheduleListResult(
    IReadOnlyList<CommissionScheduleListItemDto> Schedules,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

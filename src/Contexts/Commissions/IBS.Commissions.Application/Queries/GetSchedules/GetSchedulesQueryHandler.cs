using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;
using IBS.Commissions.Domain.Queries;

namespace IBS.Commissions.Application.Queries.GetSchedules;

/// <summary>
/// Handler for the GetSchedulesQuery.
/// </summary>
public sealed class GetSchedulesQueryHandler(
    ICommissionScheduleQueries scheduleQueries) : IQueryHandler<GetSchedulesQuery, ScheduleListResult>
{
    /// <inheritdoc />
    public async Task<Result<ScheduleListResult>> Handle(GetSchedulesQuery request, CancellationToken cancellationToken)
    {
        var filter = new ScheduleSearchFilter
        {
            SearchTerm = request.SearchTerm,
            CarrierId = request.CarrierId,
            LineOfBusiness = request.LineOfBusiness,
            IsActive = request.IsActive,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };

        var result = await scheduleQueries.SearchAsync(filter, cancellationToken);

        var items = result.Schedules.Select(s => new CommissionScheduleListItemDto(
            s.Id,
            s.CarrierId,
            s.CarrierName,
            s.LineOfBusiness,
            s.NewBusinessRate,
            s.RenewalRate,
            s.EffectiveFrom,
            s.EffectiveTo,
            s.IsActive,
            s.CreatedAt
        )).ToList();

        return new ScheduleListResult(
            items,
            result.TotalCount,
            result.PageNumber,
            result.PageSize,
            result.TotalPages
        );
    }
}

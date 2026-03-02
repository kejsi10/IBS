using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;
using IBS.Commissions.Domain.Queries;

namespace IBS.Commissions.Application.Queries.GetScheduleById;

/// <summary>
/// Handler for the GetScheduleByIdQuery.
/// </summary>
public sealed class GetScheduleByIdQueryHandler(
    ICommissionScheduleQueries scheduleQueries) : IQueryHandler<GetScheduleByIdQuery, CommissionScheduleDto>
{
    /// <inheritdoc />
    public async Task<Result<CommissionScheduleDto>> Handle(GetScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        var schedule = await scheduleQueries.GetByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule is null)
            return Error.NotFound("Commission schedule not found.");

        return new CommissionScheduleDto(
            schedule.Id,
            schedule.CarrierId,
            schedule.CarrierName,
            schedule.LineOfBusiness,
            schedule.NewBusinessRate,
            schedule.RenewalRate,
            schedule.EffectiveFrom,
            schedule.EffectiveTo,
            schedule.IsActive,
            schedule.CreatedAt,
            schedule.UpdatedAt
        );
    }
}

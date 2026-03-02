using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;

namespace IBS.Commissions.Application.Queries.GetScheduleById;

/// <summary>
/// Query to get a commission schedule by its identifier.
/// </summary>
public sealed record GetScheduleByIdQuery(
    Guid TenantId,
    Guid ScheduleId
) : IQuery<CommissionScheduleDto>;

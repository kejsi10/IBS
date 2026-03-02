using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;

namespace IBS.Commissions.Application.Queries.GetProducerReport;

/// <summary>
/// Query to get producer report by producer and period.
/// </summary>
public sealed record GetProducerReportQuery(
    Guid TenantId,
    Guid? ProducerId = null,
    int? PeriodMonth = null,
    int? PeriodYear = null
) : IQuery<IReadOnlyList<ProducerReportEntryDto>>;

using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;

namespace IBS.Commissions.Application.Queries.GetStatementById;

/// <summary>
/// Query to get a commission statement by its identifier.
/// </summary>
public sealed record GetStatementByIdQuery(
    Guid TenantId,
    Guid StatementId
) : IQuery<CommissionStatementDto>;

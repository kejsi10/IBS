using IBS.BuildingBlocks.Application.Queries;
using IBS.Commissions.Application.DTOs;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Application.Queries.GetStatements;

/// <summary>
/// Query to search and list commission statements.
/// </summary>
public sealed record GetStatementsQuery(
    Guid TenantId,
    string? SearchTerm = null,
    Guid? CarrierId = null,
    StatementStatus? Status = null,
    int? PeriodMonth = null,
    int? PeriodYear = null,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "CreatedAt",
    string SortDirection = "desc"
) : IQuery<StatementListResult>;

/// <summary>
/// Result for statement list query.
/// </summary>
public sealed record StatementListResult(
    IReadOnlyList<CommissionStatementListItemDto> Statements,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);

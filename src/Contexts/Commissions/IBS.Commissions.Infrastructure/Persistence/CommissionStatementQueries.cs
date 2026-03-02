using IBS.Commissions.Domain.Aggregates.CommissionStatement;
using IBS.Commissions.Domain.Queries;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Commissions.Infrastructure.Persistence;

/// <summary>
/// Read-side query implementation for commission statements.
/// </summary>
public sealed class CommissionStatementQueries : ICommissionStatementQueries
{
    private readonly DbSet<CommissionStatement> _statements;

    /// <summary>
    /// Initializes a new instance of the CommissionStatementQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CommissionStatementQueries(DbContext context)
    {
        _statements = context.Set<CommissionStatement>();
    }

    /// <inheritdoc />
    public async Task<CommissionStatementReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var statement = await _statements
            .AsNoTracking()
            .Include(s => s.LineItems)
            .Include(s => s.ProducerSplits)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (statement is null)
            return null;

        return MapToReadModel(statement);
    }

    /// <inheritdoc />
    public async Task<StatementSearchResult> SearchAsync(StatementSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _statements.AsNoTracking()
            .Include(s => s.LineItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(s => s.CarrierName.ToLower().Contains(term) ||
                                     s.StatementNumber.ToLower().Contains(term));
        }

        if (filter.CarrierId.HasValue)
            query = query.Where(s => s.CarrierId == filter.CarrierId.Value);

        if (filter.Status.HasValue)
            query = query.Where(s => s.Status == filter.Status.Value);

        if (filter.PeriodMonth.HasValue)
            query = query.Where(s => s.PeriodMonth == filter.PeriodMonth.Value);

        if (filter.PeriodYear.HasValue)
            query = query.Where(s => s.PeriodYear == filter.PeriodYear.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "carriername" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.CarrierName)
                : query.OrderByDescending(s => s.CarrierName),
            "statementnumber" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.StatementNumber)
                : query.OrderByDescending(s => s.StatementNumber),
            "statementdate" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.StatementDate)
                : query.OrderByDescending(s => s.StatementDate),
            "status" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.Status)
                : query.OrderByDescending(s => s.Status),
            _ => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(s => s.CreatedAt)
                : query.OrderByDescending(s => s.CreatedAt)
        };

        var statements = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new StatementSearchResult
        {
            Statements = statements.Select(s => new StatementListItemReadModel(
                s.Id,
                s.CarrierId,
                s.CarrierName,
                s.StatementNumber,
                s.PeriodMonth,
                s.PeriodYear,
                s.StatementDate,
                s.Status.GetDisplayName(),
                s.TotalPremium.Amount,
                s.TotalPremium.Currency,
                s.TotalCommission.Amount,
                s.TotalCommission.Currency,
                s.LineItems.Count,
                s.LineItems.Count(li => li.IsReconciled),
                s.LineItems.Count(li => li.DisputeReason != null),
                s.ReceivedAt,
                s.CreatedAt
            )).ToList(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<CommissionStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var statements = await _statements.AsNoTracking()
            .Include(s => s.LineItems)
            .ToListAsync(cancellationToken);

        var totalStatements = statements.Count;
        var receivedStatements = statements.Count(s => s.Status == StatementStatus.Received);
        var reconcilingStatements = statements.Count(s => s.Status == StatementStatus.Reconciling);
        var reconciledStatements = statements.Count(s => s.Status == StatementStatus.Reconciled);
        var paidStatements = statements.Count(s => s.Status == StatementStatus.Paid);
        var disputedStatements = statements.Count(s => s.Status == StatementStatus.Disputed);

        var totalCommission = statements.Sum(s => s.TotalCommission.Amount);
        var totalPaid = statements
            .Where(s => s.Status == StatementStatus.Paid)
            .Sum(s => s.TotalCommission.Amount);
        var totalDisputed = statements
            .Where(s => s.Status == StatementStatus.Disputed)
            .Sum(s => s.TotalCommission.Amount);

        var statementsByStatus = statements
            .GroupBy(s => s.Status.GetDisplayName())
            .ToDictionary(g => g.Key, g => g.Count());

        var commissionByCarrier = statements
            .GroupBy(s => s.CarrierName)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.TotalCommission.Amount));

        return new CommissionStatistics(
            totalStatements,
            receivedStatements,
            reconcilingStatements,
            reconciledStatements,
            paidStatements,
            disputedStatements,
            totalCommission,
            totalPaid,
            totalDisputed,
            statementsByStatus,
            commissionByCarrier
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CommissionSummaryEntry>> GetSummaryReportAsync(
        Guid? carrierId = null,
        int? periodMonth = null,
        int? periodYear = null,
        CancellationToken cancellationToken = default)
    {
        var query = _statements.AsNoTracking().AsQueryable();

        if (carrierId.HasValue)
            query = query.Where(s => s.CarrierId == carrierId.Value);

        if (periodMonth.HasValue)
            query = query.Where(s => s.PeriodMonth == periodMonth.Value);

        if (periodYear.HasValue)
            query = query.Where(s => s.PeriodYear == periodYear.Value);

        var statements = await query.ToListAsync(cancellationToken);

        return statements
            .GroupBy(s => new { s.CarrierId, s.CarrierName, s.PeriodMonth, s.PeriodYear })
            .Select(g => new CommissionSummaryEntry(
                g.Key.CarrierId,
                g.Key.CarrierName,
                g.Key.PeriodMonth,
                g.Key.PeriodYear,
                g.Count(),
                g.Sum(s => s.TotalPremium.Amount),
                g.Sum(s => s.TotalCommission.Amount),
                g.Where(s => s.Status == StatementStatus.Paid).Sum(s => s.TotalCommission.Amount),
                g.First().TotalCommission.Currency
            ))
            .OrderByDescending(e => e.PeriodYear)
            .ThenByDescending(e => e.PeriodMonth)
            .ThenBy(e => e.CarrierName)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProducerReportEntry>> GetProducerReportAsync(
        Guid? producerId = null,
        int? periodMonth = null,
        int? periodYear = null,
        CancellationToken cancellationToken = default)
    {
        var query = _statements.AsNoTracking()
            .Include(s => s.ProducerSplits)
            .AsQueryable();

        if (periodMonth.HasValue)
            query = query.Where(s => s.PeriodMonth == periodMonth.Value);

        if (periodYear.HasValue)
            query = query.Where(s => s.PeriodYear == periodYear.Value);

        var statements = await query.ToListAsync(cancellationToken);

        var allSplits = statements
            .SelectMany(s => s.ProducerSplits.Select(ps => new { Statement = s, Split = ps }))
            .ToList();

        if (producerId.HasValue)
            allSplits = allSplits.Where(x => x.Split.ProducerId == producerId.Value).ToList();

        return allSplits
            .GroupBy(x => new { x.Split.ProducerId, x.Split.ProducerName, x.Statement.PeriodMonth, x.Statement.PeriodYear })
            .Select(g => new ProducerReportEntry(
                g.Key.ProducerId,
                g.Key.ProducerName,
                g.Key.PeriodMonth,
                g.Key.PeriodYear,
                g.Count(),
                g.Sum(x => x.Split.SplitAmount.Amount),
                g.Average(x => x.Split.SplitPercentage),
                g.First().Split.SplitAmount.Currency
            ))
            .OrderByDescending(e => e.PeriodYear)
            .ThenByDescending(e => e.PeriodMonth)
            .ThenBy(e => e.ProducerName)
            .ToList();
    }

    private static CommissionStatementReadModel MapToReadModel(CommissionStatement statement)
    {
        return new CommissionStatementReadModel(
            statement.Id,
            statement.CarrierId,
            statement.CarrierName,
            statement.StatementNumber,
            statement.PeriodMonth,
            statement.PeriodYear,
            statement.StatementDate,
            statement.Status.GetDisplayName(),
            statement.TotalPremium.Amount,
            statement.TotalPremium.Currency,
            statement.TotalCommission.Amount,
            statement.TotalCommission.Currency,
            statement.ReceivedAt,
            statement.CreatedAt,
            statement.UpdatedAt,
            statement.LineItems.Select(li => new LineItemReadModel(
                li.Id, li.PolicyId, li.PolicyNumber, li.InsuredName, li.LineOfBusiness,
                li.EffectiveDate, li.TransactionType.GetDisplayName(),
                li.GrossPremium.Amount, li.GrossPremium.Currency,
                li.CommissionRate, li.CommissionAmount.Amount, li.CommissionAmount.Currency,
                li.IsReconciled, li.ReconciledAt, li.DisputeReason)).ToList(),
            statement.ProducerSplits.Select(ps => new ProducerSplitReadModel(
                ps.Id, ps.LineItemId, ps.ProducerName, ps.ProducerId,
                ps.SplitPercentage, ps.SplitAmount.Amount, ps.SplitAmount.Currency)).ToList(),
            Convert.ToBase64String(statement.RowVersion)
        );
    }
}

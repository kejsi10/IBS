using IBS.Claims.Domain.Aggregates.Claim;
using IBS.Claims.Domain.Queries;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Claims.Infrastructure.Persistence;

/// <summary>
/// Read-side query implementation for claims.
/// </summary>
public sealed class ClaimQueries : IClaimQueries
{
    private readonly DbContext _context;
    private readonly DbSet<Claim> _claims;

    /// <summary>
    /// Initializes a new instance of the ClaimQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ClaimQueries(DbContext context)
    {
        _context = context;
        _claims = context.Set<Claim>();
    }

    /// <inheritdoc />
    public async Task<ClaimReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var claim = await _claims
            .AsNoTracking()
            .Include(c => c.Notes)
            .Include(c => c.Reserves)
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (claim is null)
            return null;

        return MapToReadModel(claim);
    }

    /// <inheritdoc />
    public async Task<ClaimSearchResult> SearchAsync(ClaimSearchFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _claims.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(c => c.ClaimNumber.Value.ToLower().Contains(term) ||
                                     c.LossDescription.ToLower().Contains(term));
        }

        if (filter.Status.HasValue)
            query = query.Where(c => c.Status == filter.Status.Value);

        if (filter.PolicyId.HasValue)
            query = query.Where(c => c.PolicyId == filter.PolicyId.Value);

        if (filter.ClientId.HasValue)
            query = query.Where(c => c.ClientId == filter.ClientId.Value);

        if (filter.LossType.HasValue)
            query = query.Where(c => c.LossType == filter.LossType.Value);

        if (filter.LossDateFrom.HasValue)
            query = query.Where(c => c.LossDate >= filter.LossDateFrom.Value);

        if (filter.LossDateTo.HasValue)
            query = query.Where(c => c.LossDate <= filter.LossDateTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortBy?.ToLower() switch
        {
            "claimnumber" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(c => c.ClaimNumber.Value)
                : query.OrderByDescending(c => c.ClaimNumber.Value),
            "lossdate" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(c => c.LossDate)
                : query.OrderByDescending(c => c.LossDate),
            "reporteddate" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(c => c.ReportedDate)
                : query.OrderByDescending(c => c.ReportedDate),
            "status" => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(c => c.Status)
                : query.OrderByDescending(c => c.Status),
            _ => filter.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt)
        };

        var claims = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new ClaimSearchResult
        {
            Claims = claims.Select(c => new ClaimListItemReadModel(
                c.Id,
                c.ClaimNumber.Value,
                c.PolicyId,
                c.ClientId,
                c.Status.GetDisplayName(),
                c.LossDate,
                c.ReportedDate,
                c.LossType.GetDisplayName(),
                c.LossAmount?.Amount,
                c.LossAmount?.Currency,
                c.ClaimAmount?.Amount,
                c.ClaimAmount?.Currency,
                c.AssignedAdjusterId,
                c.CreatedAt
            )).ToList(),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    /// <inheritdoc />
    public async Task<ClaimStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var claims = await _claims.AsNoTracking()
            .Include(c => c.Payments)
            .ToListAsync(cancellationToken);

        var totalClaims = claims.Count;
        var openClaims = claims.Count(c => c.Status.IsOpen());
        var closedClaims = claims.Count(c => c.Status == ClaimStatus.Closed);
        var deniedClaims = claims.Count(c => c.Status == ClaimStatus.Denied);

        var totalClaimAmount = claims
            .Where(c => c.ClaimAmount != null)
            .Sum(c => c.ClaimAmount!.Amount);

        var totalPaidAmount = claims
            .SelectMany(c => c.Payments)
            .Where(p => p.Status == PaymentStatus.Issued)
            .Sum(p => p.Amount.Amount);

        var claimsByStatus = claims
            .GroupBy(c => c.Status.GetDisplayName())
            .ToDictionary(g => g.Key, g => g.Count());

        var claimsByLossType = claims
            .GroupBy(c => c.LossType.GetDisplayName())
            .ToDictionary(g => g.Key, g => g.Count());

        return new ClaimStatistics(
            totalClaims,
            openClaims,
            closedClaims,
            deniedClaims,
            totalClaimAmount,
            totalPaidAmount,
            claimsByStatus,
            claimsByLossType
        );
    }

    private static ClaimReadModel MapToReadModel(Claim claim)
    {
        return new ClaimReadModel(
            claim.Id,
            claim.ClaimNumber.Value,
            claim.PolicyId,
            claim.ClientId,
            claim.Status.GetDisplayName(),
            claim.LossDate,
            claim.ReportedDate,
            claim.LossType.GetDisplayName(),
            claim.LossDescription,
            claim.LossAmount?.Amount,
            claim.LossAmount?.Currency,
            claim.ClaimAmount?.Amount,
            claim.ClaimAmount?.Currency,
            claim.AssignedAdjusterId,
            claim.DenialReason,
            claim.ClosedAt,
            claim.ClosureReason,
            claim.CreatedBy,
            claim.CreatedAt,
            claim.UpdatedAt,
            claim.Notes.Select(n => new ClaimNoteReadModel(
                n.Id, n.Content, n.AuthorBy, n.IsInternal, n.CreatedAt)).ToList(),
            claim.Reserves.Select(r => new ReserveReadModel(
                r.Id, r.ReserveType, r.Amount.Amount, r.Amount.Currency, r.SetBy, r.SetAt, r.Notes)).ToList(),
            claim.Payments.Select(p => new ClaimPaymentReadModel(
                p.Id, p.PaymentType, p.Amount.Amount, p.Amount.Currency, p.PayeeName, p.PaymentDate,
                p.CheckNumber, p.Status.ToString(), p.AuthorizedBy, p.AuthorizedAt, p.IssuedAt, p.VoidedAt, p.VoidReason)).ToList(),
            Convert.ToBase64String(claim.RowVersion)
        );
    }
}
